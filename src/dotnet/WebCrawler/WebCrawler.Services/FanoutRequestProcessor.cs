using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.Storage.Queues;
using WebCrawler.Model;
using WebCrawler.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebCrawler.Services
{
	public class FanoutRequestProcessor : IFanoutRequestProcessor
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<FanoutRequestProcessor> _logger;
        private readonly QueueConfig _queueConfig;
        private readonly IGitHubProjectService _gitHubProjectService;

        public FanoutRequestProcessor(ILogger<FanoutRequestProcessor> log, QueueConfig queueConfig, IGitHubProjectService gitHubProjectService)
        {
            _logger = log;
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            _telemetryClient = new TelemetryClient(telemetryConfig);
            _queueConfig = queueConfig;
            _gitHubProjectService = gitHubProjectService;
        }

        public async Task ProcessGitHubUserAsync(string username)
        {
            var allProjects = _gitHubProjectService.GetProjects();

            foreach(var project in allProjects)
            {
                var providerRequest = new ProcessGitHubUserProviderRequest()
                {
                    Owner = project.Owner,
                    Repo = project.Repo,
                    UserName = username
                };
                await ProcessGitHubUserContributionAsync(providerRequest);
            }
        }

        public async Task ProcessGitHubUserContributionAsync(ProcessGitHubUserProviderRequest githubUserContributionRequest)
        {
            var queueMessage = JsonConvert.SerializeObject(githubUserContributionRequest);

            var queueClient = new QueueClient(_queueConfig.ConnectionString, QueueNames.GITHUB_USER_PROVIDER, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queueClient.SendMessageAsync(queueMessage);
        }
    }
}