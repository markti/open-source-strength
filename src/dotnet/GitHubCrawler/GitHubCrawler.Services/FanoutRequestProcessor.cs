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
        private readonly IGitHubProjectService _gitHubProjectService;

        public FanoutRequestProcessor(
            ILogger<FanoutRequestProcessor> log,
            TelemetryConfiguration telemetryConfiguration,
            QueueConfig queueConfig,
            IGitHubProjectService gitHubProjectService
            )
        {
            _logger = log;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _queueConfig = queueConfig;
            _gitHubProjectService = gitHubProjectService;
        }

        public async Task ProcessRepoPullRequestHistoryAsync(ProcessRepositoryPageRequest repoPageRequest)
        {
            var queueMessage = JsonConvert.SerializeObject(repoPageRequest);

            var queueClient = new QueueClient(_queueConfig.ConnectionString, QueueNames.PULL_REQUEST_PAGE_QUEUE_NAME, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            await queueClient.SendMessageAsync(queueMessage);
        }


        public async Task ProcessGitHubUserAsync(string username)
        {
            var allProjects = _gitHubProjectService.GetProjects();

            foreach (var project in allProjects)
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