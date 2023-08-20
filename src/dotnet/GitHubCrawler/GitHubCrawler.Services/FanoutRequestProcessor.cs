using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.Storage.Queues;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubCrawler.Services
{
	public class FanoutRequestProcessor : IFanoutRequestProcessor
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<FanoutRequestProcessor> _logger;
        private readonly QueueConfig _queueConfig;

        public FanoutRequestProcessor(ILogger<FanoutRequestProcessor> log, QueueConfig queueConfig)
        {
            _logger = log;
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            _telemetryClient = new TelemetryClient(telemetryConfig);
            _queueConfig = queueConfig;
        }

        public async Task ProcessRepoPullRequestHistoryAsync(ProcessRepositoryPageRequest processRepoRequest)
        {
            var queueMessage = JsonConvert.SerializeObject(processRepoRequest);

            var queueClient = new QueueClient(_queueConfig.ConnectionString, QueueNames.PULL_REQUEST_PAGE_QUEUE_NAME, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queueClient.SendMessageAsync(queueMessage);
        }
    }
}