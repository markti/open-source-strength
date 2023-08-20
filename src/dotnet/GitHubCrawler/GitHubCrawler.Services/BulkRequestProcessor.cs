using System;
using System.Reflection.Metadata;
using System.Text;
using Azure.Storage.Blobs;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubCrawler.Services
{
	public class BulkRequestProcessor : IBulkRequestProcessor
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<BulkRequestProcessor> _logger;
        private readonly QueueConfig _queueConfig;
        private readonly BlobConfig _blobConfig;
        private readonly IGitHubQueryService _githubQueryService;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;

        public BulkRequestProcessor(
            ILogger<BulkRequestProcessor> logger,
            QueueConfig queueConfig,
            BlobConfig blobConfig,
            IFanoutRequestProcessor fanoutRequestProcessor,
            IGitHubQueryService githubQueryService
            )
        {
            _logger = logger;
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            _telemetryClient = new TelemetryClient(telemetryConfig);
            _queueConfig = queueConfig;
            _blobConfig = blobConfig;
            _fanoutRequestProcessor = fanoutRequestProcessor;
            _githubQueryService = githubQueryService;
        }

        public async Task ProcessPullRequestPageRequest(ProcessRepositoryPageRequest repoPageRequest)
        {
            var list = await _githubQueryService.GetPullRequestHistory(repoPageRequest);

            var pageData = JsonConvert.SerializeObject(list);

            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerNames.PULL_REQUESTS);

            var blobName = $"{repoPageRequest.Owner}/{repoPageRequest.Repo}/{repoPageRequest.PageNumber}.json";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pageData));
            await blobClient.UploadAsync(stream, overwrite: true);

            var eventProperties = new Dictionary<string, string>();
            eventProperties.Add("owner", repoPageRequest.Owner);
            eventProperties.Add("repo", repoPageRequest.Repo);
            eventProperties.Add("page", repoPageRequest.PageNumber.ToString());

            _telemetryClient.TrackEvent("github-pull-request-page", eventProperties);

            // if we have items that come back, do another page
            if (list.Count > 0)
            {
                var nextPageRequest = new ProcessRepositoryPageRequest()
                {
                    Owner = repoPageRequest.Owner,
                    Repo = repoPageRequest.Repo,
                    PageNumber = repoPageRequest.PageNumber + 1
                };
                await _fanoutRequestProcessor.ProcessRepoPullRequestHistoryAsync(nextPageRequest);
            }
        }
    }
}