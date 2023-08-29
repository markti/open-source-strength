using Xunit.Abstractions;
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
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace GitHubCrawler.IntegrationTests;

public class UnitTest1
{

    private readonly ILogger<PageProcessor> _logger;
    private readonly ILogger<CompanyMemberService> _companyMemberServiceLogger;
    private readonly Mock<IFanoutRequestProcessor> _mockFanoutProcessor;
    private readonly IConfiguration _configuration;
    private readonly TelemetryConfiguration _telemetryConfiguration;
    private readonly BlobConfig _blobConfig;

    public UnitTest1(ITestOutputHelper output)
    {
        _logger = new XunitLogger<PageProcessor>(output);
        _companyMemberServiceLogger = new XunitLogger<CompanyMemberService>(output);
        _mockFanoutProcessor = new Mock<IFanoutRequestProcessor>();
        _mockFanoutProcessor.Setup(f => f.ProcessGitHubUserAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

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
    }

    [Trait("Category", "Integration")]
    [Fact]
    public async Task Test1()
    {
        var githubProjectService = new GitHubProjectService();
        var companyMemberService = new CompanyMemberService(_companyMemberServiceLogger, _telemetryConfiguration, _blobConfig);

        var pageProcessor = new PageProcessor(_logger, _telemetryConfiguration, _blobConfig, companyMemberService, _mockFanoutProcessor.Object);

        var cosignerSummary = await pageProcessor.ProcessPageAsync();

        Assert.NotNull(cosignerSummary);
    }
}