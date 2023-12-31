﻿using System;
using System.Reflection.Metadata;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;

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
            TelemetryConfiguration telemetryConfiguration,
            QueueConfig queueConfig,
            BlobConfig blobConfig,
            IFanoutRequestProcessor fanoutRequestProcessor,
            IGitHubQueryService githubQueryService
            )
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
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

        public async Task<int> CalculatePullRequestCountAsync(string owner, string repo)
        {
            var matchingPullRequests = 0;
            var currentPageNumber = 1;
            var shouldContinue = true;

            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerNames.PULL_REQUESTS);

            while (shouldContinue)
            {
                var blobName = $"{owner}/{repo}/{currentPageNumber}.json";
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                var blobExistsResult = (await blobClient.ExistsAsync()).Value;
                if (!blobExistsResult)
                {
                    shouldContinue = false;
                }
                else
                {
                    BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                    string blobContents = downloadResult.Content.ToString();

                    // Deserialize blob contents to FooBar object
                    var prSummary = JsonConvert.DeserializeObject<List<PullRequestSummary>>(blobContents);

                    if (prSummary == null)
                    {
                    }
                    else
                    {
                        var pullRequestsInCurrentBatch = prSummary.Count();
                        matchingPullRequests += pullRequestsInCurrentBatch;
                    }
                    currentPageNumber++;
                }
            }

            return matchingPullRequests;
        }

        public async Task<int> ProcessGitHubUserContributionRequest(ProcessGitHubUserProviderRequest userRequest)
        {
            var eventProperties = new Dictionary<string, string>();
            eventProperties.Add("owner", userRequest.Owner);
            eventProperties.Add("repo", userRequest.Repo);
            eventProperties.Add("username", userRequest.UserName);

            _telemetryClient.TrackEvent("user", eventProperties);

            var matchingPullRequests = 0;
            var currentPageNumber = 1;
            var shouldContinue = true;

            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerNames.PULL_REQUESTS);

            while(shouldContinue)
            {
                var blobName = $"{userRequest.Owner}/{userRequest.Repo}/{currentPageNumber}.json";
                _logger.LogInformation($"Checking file {blobName} for user {userRequest.UserName}");
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                var blobExistsResult = (await blobClient.ExistsAsync()).Value;
                if (!blobExistsResult)
                {
                    shouldContinue = false;
                }
                else
                {
                    BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                    string blobContents = downloadResult.Content.ToString();

                    // Deserialize blob contents to FooBar object
                    var prSummary = JsonConvert.DeserializeObject<List<PullRequestSummary>>(blobContents);

                    if (prSummary == null)
                    {
                    }
                    else
                    {
                        var pullRequestsInCurrentBatch = prSummary.Where(f => f.UserName == userRequest.UserName).Count();
                        matchingPullRequests += pullRequestsInCurrentBatch;
                    }
                    currentPageNumber++;
                }
            }

            return matchingPullRequests;
        }

        public async Task<int> ProcessPullRequestTotal(string owner, string repo)
        {
            var eventProperties = new Dictionary<string, string>();
            eventProperties.Add("owner", owner);
            eventProperties.Add("repo", repo);

            _telemetryClient.TrackEvent("repo", eventProperties);

            var matchingPullRequests = 0;
            var currentPageNumber = 1;
            var shouldContinue = true;

            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerNames.PULL_REQUESTS);

            while (shouldContinue)
            {
                var blobName = $"{owner}/{repo}/{currentPageNumber}.json";
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                var blobExistsResult = (await blobClient.ExistsAsync()).Value;
                if (!blobExistsResult)
                {
                    shouldContinue = false;
                }
                else
                {
                    BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                    string blobContents = downloadResult.Content.ToString();

                    // Deserialize blob contents to FooBar object
                    var prSummary = JsonConvert.DeserializeObject<List<PullRequestSummary>>(blobContents);

                    if (prSummary == null)
                    {
                    }
                    else
                    {
                        var pullRequestsInCurrentBatch = prSummary.Count();
                        matchingPullRequests += pullRequestsInCurrentBatch;
                    }
                    currentPageNumber++;
                }
            }

            return matchingPullRequests;
        }

        public async Task<int> GetTotalPullRequestCountAsync(string owner, string repo)
        {

            var containerName = BlobContainerNames.PULL_REQUESTS;
            var blobName = $"{owner}/{repo}/total.txt";
            var blobContent = await ReadFromBlobAsync(containerName, blobName);

            var totalCount = 0;

            int.TryParse(blobContent, out totalCount);

            return totalCount;
        }

        private async Task<string> ReadFromBlobAsync(string containerName, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            string blobContents = downloadResult.Content.ToString();
            return blobContents;
        }

        public async Task SaveAsync(string containerName, string blobName, string blobContent)
        {
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
            await blobClient.UploadAsync(stream, overwrite: true);
        }
    }
}