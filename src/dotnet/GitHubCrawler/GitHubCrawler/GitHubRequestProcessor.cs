using System;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using GitHubCrawler.Model;
using GitHubCrawler.Services;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubCrawler
{
    public class GitHubRequestProcessor
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<GitHubRequestProcessor> _logger;
        private readonly IBulkRequestProcessor _bulkRequestProcessor;
        private readonly QueueConfig _queueConfig;

        public GitHubRequestProcessor(
            ILogger<GitHubRequestProcessor> logger,
            TelemetryConfiguration telemetryConfiguration,
            IBulkRequestProcessor bulkRequestProcessor,
            QueueConfig queueConfig
            )
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _bulkRequestProcessor = bulkRequestProcessor;
            _queueConfig = queueConfig;
        }

        [FunctionName("pull-request-page")]
        public async Task Run([QueueTrigger(QueueNames.PULL_REQUEST_PAGE_QUEUE_NAME, Connection = "QUEUE_CONNECTION_STRING")]string queueMessage)
        {
            _logger.LogInformation("Reading Pull Request Page Request Queue Item");
            var dataRequest = JsonConvert.DeserializeObject<ProcessRepositoryPageRequest>(queueMessage);
            await _bulkRequestProcessor.ProcessPullRequestPageRequest(dataRequest);
        }
        [FunctionName("user-contributions")]
        public async Task ProcessUserContributions([QueueTrigger(QueueNames.GITHUB_USER_PROVIDER, Connection = "QUEUE_CONNECTION_STRING")] string queueMessage)
        {
            _logger.LogInformation("Reading Pull Request Page Request Queue Item");
            var dataRequest = JsonConvert.DeserializeObject<ProcessGitHubUserProviderRequest>(queueMessage);
            var matchingPullRequests = await _bulkRequestProcessor.ProcessGitHubUserContributionRequest(dataRequest);

            if (matchingPullRequests > 0)
            {
                var blobName = $"{dataRequest.UserName}/{dataRequest.Owner}/{dataRequest.Repo}.json";
                var blobContent = matchingPullRequests.ToString();
                _logger.LogInformation($"User Contributor {dataRequest.UserName} has {matchingPullRequests} PRs for {dataRequest.Owner}/{dataRequest.Repo}");

                await _bulkRequestProcessor.SaveAsync(BlobContainerNames.USERS, blobName, blobContent);
            }
        }
    }
}