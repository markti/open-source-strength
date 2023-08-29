using System;
using GitHubCrawler.Model;
using GitHubCrawler.Services;
using GitHubCrawler.Services.Interfaces;
using GitHubCrawler.TestFramework;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace GitHubCrawler.UnitTests
{
	public class BulkRequestProcessorTests
    {
        private readonly ILogger<BulkRequestProcessor> _logger;
        private readonly IConfiguration _configuration;
        private readonly TelemetryConfiguration _telemetryConfiguration;
        private readonly QueueConfig _queueConfig;
        private readonly BlobConfig _blobConfig;
        private readonly Mock<IFanoutRequestProcessor> _fanoutRequestProcessorMock;
        private readonly Mock<IGitHubQueryService> _githubQueryServiceMock;
        
        public BulkRequestProcessorTests(ITestOutputHelper output)
        {
            _logger = new XunitLogger<BulkRequestProcessor>(output);

            var configBuilder = new ConfigurationBuilder()
            .AddUserSecrets<BulkRequestProcessorTests>()  // Using the test class to get the assembly for user-secrets
            .Build();

            _configuration = configBuilder;

            _telemetryConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new MockTelemetryChannel(),
                InstrumentationKey = Guid.NewGuid().ToString()
            };
            _telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            _fanoutRequestProcessorMock = new Mock<IFanoutRequestProcessor>();
            _githubQueryServiceMock = new Mock<IGitHubQueryService>();

            _queueConfig = new QueueConfig()
            {
                ConnectionString = _configuration["QUEUE_CONNECTION_STRING"]
            };
            _blobConfig = new BlobConfig()
            {
                ConnectionString = _configuration["STORAGE_CONNECTION_STRING"]
            };
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task MitchellH_PullRequests()
        {

            var request = new ProcessGitHubUserProviderRequest()
            {
                Owner = "hashicorp",
                Repo = "terraform",
                UserName = "mitchellh"
            };
            var expectedPullRequestCount = 390;

            var service = new BulkRequestProcessor(_logger, _telemetryConfiguration, _queueConfig, _blobConfig, _fanoutRequestProcessorMock.Object, _githubQueryServiceMock.Object);

            var actualPullRequestCount = await service.ProcessGitHubUserContributionRequest(request);

            Assert.Equal(expectedPullRequestCount, actualPullRequestCount);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task jbardin_PullRequests()
        {

            var request = new ProcessGitHubUserProviderRequest()
            {
                Owner = "hashicorp",
                Repo = "terraform",
                UserName = "jbardin"
            };
            var expectedPullRequestCount = 921;

            var service = new BulkRequestProcessor(_logger, _telemetryConfiguration, _queueConfig, _blobConfig, _fanoutRequestProcessorMock.Object, _githubQueryServiceMock.Object);

            var actualPullRequestCount = await service.ProcessGitHubUserContributionRequest(request);

            Assert.Equal(expectedPullRequestCount, actualPullRequestCount);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task apparentlymart_PullRequests()
        {

            var request = new ProcessGitHubUserProviderRequest()
            {
                Owner = "hashicorp",
                Repo = "terraform",
                UserName = "apparentlymart"
            };
            var expectedPullRequestCount = 645;

            var service = new BulkRequestProcessor(_logger, _telemetryConfiguration, _queueConfig, _blobConfig, _fanoutRequestProcessorMock.Object, _githubQueryServiceMock.Object);

            var actualPullRequestCount = await service.ProcessGitHubUserContributionRequest(request);

            Assert.Equal(expectedPullRequestCount, actualPullRequestCount);
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task Terraform_Total_PullRequests()
        {
            string owner = "hashicorp";
            string repo = "terraform";
            var expectedPullRequestCount = 10871;

            var service = new BulkRequestProcessor(_logger, _telemetryConfiguration, _queueConfig, _blobConfig, _fanoutRequestProcessorMock.Object, _githubQueryServiceMock.Object);

            var actualPullRequestCount = await service.ProcessPullRequestTotal(owner, repo);

            Assert.Equal(expectedPullRequestCount, actualPullRequestCount);
        }
    }
}