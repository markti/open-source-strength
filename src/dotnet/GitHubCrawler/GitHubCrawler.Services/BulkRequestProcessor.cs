using System;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace GitHubCrawler.Services
{
	public class BulkRequestProcessor : IBulkRequestProcessor
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<BulkRequestProcessor> _logger;
        private readonly QueueConfig _queueConfig;

        public BulkRequestProcessor(ILogger<BulkRequestProcessor> logger, QueueConfig queueConfig)
        {
            _logger = logger;
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            _telemetryClient = new TelemetryClient(telemetryConfig);
            _queueConfig = queueConfig;
        }

        public async Task ProcessPullRequestPageRequest(ProcessRepositoryPageRequest processRepoRequest)
        {
        }
    }
}