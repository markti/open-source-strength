using System;
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

        public GitHubRequestProcessor(ILogger<GitHubRequestProcessor> logger, IBulkRequestProcessor bulkRequestProcessor, QueueConfig queueConfig)
        {
            _logger = logger;
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            _telemetryClient = new TelemetryClient(telemetryConfig);
            _bulkRequestProcessor = bulkRequestProcessor;
            _queueConfig = queueConfig;
        }

        [FunctionName("pull-request-page")]
        public void Run([QueueTrigger(QueueNames.PULL_REQUEST_PAGE_QUEUE_NAME, Connection = "QUEUE_CONNECTION_STRING")]string queueMessage)
        {
            _logger.LogInformation("Reading Pull Request Page Request Queue Item");
            var dataRequest = JsonConvert.DeserializeObject<ProcessRepositoryPageRequest>(queueMessage);
            _bulkRequestProcessor.ProcessPullRequestPageRequest(dataRequest);
        }
        [FunctionName("user-contributions")]
        public void ProcessUserContributions([QueueTrigger(QueueNames.GITHUB_USER_PROVIDER, Connection = "QUEUE_CONNECTION_STRING")] string queueMessage)
        {
            _logger.LogInformation("Reading Pull Request Page Request Queue Item");
            var dataRequest = JsonConvert.DeserializeObject<ProcessGitHubUserProviderRequest>(queueMessage);
            _bulkRequestProcessor.ProcessGitHubUserContributionRequest(dataRequest);
        }
    }
}