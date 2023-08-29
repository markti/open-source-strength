using System;
using Azure.Storage.Blobs;
using System.Text;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace GitHubCrawler.Services
{
	public class WebPageGenerator : IWebPageGenerator
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<WebPageGenerator> _logger;
        private readonly BlobConfig _blobConfig;
        private readonly ICompanyMemberService _companyMemberService;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;

        public WebPageGenerator(
                ILogger<WebPageGenerator> logger,
                TelemetryConfiguration telemetryConfiguration,
                BlobConfig blobConfig)
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _blobConfig = blobConfig;
        }

		public async Task<string> GenerateHtmlAsync(List<RepositorySummary> repoSummaries)
		{
			return "<h1>Hello Terraform World!!!</h1>";
        }
        public async Task SaveAsync(string webpageContent)
        {
            var containerName = "$web";
            var blobName = $"index.html";

            await SaveAsync(containerName, blobName, webpageContent);

        }

        private async Task SaveAsync(string containerName, string blobName, string blobContent)
        {
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
            await blobClient.UploadAsync(stream, overwrite: true);
        }
    }
}

