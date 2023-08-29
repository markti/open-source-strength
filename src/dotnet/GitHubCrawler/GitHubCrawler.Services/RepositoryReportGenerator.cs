using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GitHubCrawler.Services
{
    public class RepositoryReportGenerator : IRepositoryReportGenerator
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<RepositoryReportGenerator> _logger;
        private readonly BlobConfig _blobConfig;
        private readonly ICompanyMemberService _companyMemberService;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;

        public RepositoryReportGenerator(
                ILogger<RepositoryReportGenerator> logger,
                TelemetryConfiguration telemetryConfiguration,
                BlobConfig blobConfig)
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _blobConfig = blobConfig;
        }

        public async Task<RepositorySummary> GetUserContributionsAsync(string repositoryOwner, string repositoryName)
        {
            var summary = new RepositorySummary()
            {
                Owner = repositoryOwner,
                Repo = repositoryName,
                Contributors = new Dictionary<string, int>()
            };

            var contributions = new Dictionary<string, int>();

            BlobServiceClient blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("users");

            var allBlobs = containerClient.GetBlobsAsync();

            var blobFilter = $"{repositoryOwner}/{repositoryName}";

            var contributorCount = 0;
            var pullRequestCount = 0;

            await foreach(var blob in allBlobs)
            {
                _logger.LogInformation(blob.Name);
                if(blob.Name.Contains(blobFilter))
                {
                    var userName = blob.Name.Split('/')[0].Trim();

                    var blobClient = containerClient.GetBlobClient(blob.Name);
                    BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                    string blobContents = downloadResult.Content.ToString();

                    var userPullRequestCount = 0;
                    int.TryParse(blobContents, out userPullRequestCount);

                    _logger.LogInformation($"username: {userName} {blobContents}");

                    if(!summary.Contributors.ContainsKey(userName))
                    {
                        summary.Contributors.Add(userName, userPullRequestCount);
                        contributorCount++;
                        pullRequestCount += userPullRequestCount;
                    } else
                    {
                        _logger.LogWarning($"Username {userName} showed up twice for this repo {blobFilter}");
                    }
                }
            }

            summary.ContributorCount = contributorCount;
            summary.PullRequestCount = pullRequestCount;

            return summary;
        }

        public async Task SaveReportAsync(RepositorySummary report)
        {
            var containerName = BlobContainerNames.REPOS;
            var blobName = $"{report.Owner}/{report.Repo}.json";

            var jsonData = JsonConvert.SerializeObject(report);

            await SaveAsync(containerName, blobName, jsonData);

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