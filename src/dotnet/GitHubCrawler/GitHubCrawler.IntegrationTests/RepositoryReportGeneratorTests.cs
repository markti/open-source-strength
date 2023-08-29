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

namespace GitHubCrawler.IntegrationTests
{
	public class RepositoryReportGeneratorTests
    {

        private readonly ILogger<RepositoryReportGenerator> _logger;

        private readonly IConfiguration _configuration;
        private readonly TelemetryConfiguration _telemetryConfiguration;
        private readonly BlobConfig _blobConfig;
        private readonly Mock<IBulkRequestProcessor> _mockBulkRequestProcessor;

        public RepositoryReportGeneratorTests(ITestOutputHelper output)
        {
            _logger = new XunitLogger<RepositoryReportGenerator>(output);

            var configBuilder = new ConfigurationBuilder()
            .AddUserSecrets<UnitTest1>()  // Using the test class to get the assembly for user-secrets
            .Build();

            _configuration = configBuilder;

            _telemetryConfiguration = new TelemetryConfiguration
            {
                TelemetryChannel = new MockTelemetryChannel(),
                InstrumentationKey = Guid.NewGuid().ToString()
            };
            _telemetryConfiguration.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());

            _blobConfig = new BlobConfig()
            {
                ConnectionString = _configuration["STORAGE_CONNECTION_STRING"]
            };

            _mockBulkRequestProcessor = new Mock<IBulkRequestProcessor>();
        }

        [Trait("Category", "Integration")]
        [Fact]
        public async Task Test1()
        {
            var pageProcessor = new RepositoryReportGenerator(_logger, _telemetryConfiguration, _blobConfig, _mockBulkRequestProcessor.Object);

            var cosignerSummary = await pageProcessor.GetUserContributionsAsync("hashicorp", "terraform");

            Assert.NotNull(cosignerSummary);
        }
    }
}