using System;
using Azure.Storage.Blobs;
using System.Text;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs.Models;

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
            StringBuilder htmlPageBuilder = new StringBuilder();

            htmlPageBuilder.AppendLine("<!DOCTYPE html>");

            htmlPageBuilder.AppendLine("<html>");

            htmlPageBuilder.AppendLine("<head>");

            var googleHead = await ReadFromBlob("assets", "google_head.txt");
            htmlPageBuilder.AppendLine(googleHead);

            htmlPageBuilder.AppendLine("</head>");

            var googleJs = await ReadFromBlob("assets", "google_js.txt");
            htmlPageBuilder.AppendLine(googleJs);

            htmlPageBuilder.AppendLine("<body>");


            var googleBody = await ReadFromBlob("assets", "google_body.txt");
            htmlPageBuilder.AppendLine(googleHead);
            htmlPageBuilder.AppendLine("<h1>Hello Terraform World!!!</h1>");

            htmlPageBuilder.AppendLine("<ul>");
            foreach (var repo in repoSummaries)
            {
                htmlPageBuilder.Append("<li>");
                htmlPageBuilder.Append($"{repo.Owner}/{repo.Repo}: Contributors {repo.ContributorCount} submitted Pull Requests {repo.PullRequestCount} ");
                htmlPageBuilder.Append("</li>");
            }
            htmlPageBuilder.AppendLine("</ul>");

            htmlPageBuilder.AppendLine("</body>");

            htmlPageBuilder.AppendLine("</html>");

            return htmlPageBuilder.ToString();
        }
        public async Task SaveAsync(string webpageContent)
        {
            var containerName = "$web";
            var blobName = $"index.html";

            await SaveAsync(containerName, blobName, webpageContent);

        }

        private async Task<string> ReadFromBlob(string containerName, string blobName)
        {
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            string blobContents = downloadResult.Content.ToString();
            return blobContents;
        }

        private async Task SaveAsync(string containerName, string blobName, string blobContent)
        {
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = "text/html"
            };

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = httpHeaders
            };

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(blobContent));
            await blobClient.UploadAsync(stream, uploadOptions);

            _telemetryClient.TrackEvent("updated HTML");
        }
    }
}

