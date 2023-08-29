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
using Octokit;

namespace GitHubCrawler.Services
{
	public class WebPageGenerator : IWebPageGenerator
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<WebPageGenerator> _logger;
        private readonly BlobConfig _blobConfig;
        private readonly ICompanyMemberService _companyMemberService;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;
        private readonly IPageProcessor _pageProcessor;

        public WebPageGenerator(
                ILogger<WebPageGenerator> logger,
                TelemetryConfiguration telemetryConfiguration,
                BlobConfig blobConfig,
                IPageProcessor pageProcessor
                )
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _blobConfig = blobConfig;
            _pageProcessor = pageProcessor;
        }

		public async Task<string> GenerateHtmlAsync(List<RepositorySummary> repoSummaries)
		{
            var pageSummary = await _pageProcessor.GetLatestAsync();
            var contributorList = new List<string>();
            foreach (var repo in repoSummaries)
            {
                foreach(var contributor in repo.Contributors.Keys)
                {
                    if(!contributorList.Contains(contributor))
                    {
                        contributorList.Add(contributor);
                    }
                }
            }

            var activeGitHubUserCount = pageSummary.ActiveGitHubUsersCount;
            var actualContributorCount = contributorList.Count;
            var allContributionsAcrossProjects = repoSummaries.Sum(f => f.PullRequestCount);
            var allPullRequestsAcrossProjects = repoSummaries.Sum(f => f.TotalPullRequestCount);

            var pctContributionsAcrossProjects = (double)allContributionsAcrossProjects / (double)allPullRequestsAcrossProjects;
            var pctActualContributors = (double)actualContributorCount / (double)pageSummary.TotalCount;

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


            htmlPageBuilder.AppendLine($"Today is {DateTime.UtcNow.ToShortDateString()}!");

            htmlPageBuilder.AppendLine($"OpenTF was started in reaction to HashiCorp adopting BSL on several of their GitHub repositories. OpenTF has a published manifesto and encourages people to signup and promise to contribute time and/or resources to the project. The main backers of OpenTF are HashiCorp competitors but many of the signers are just regular people. The main grievence of the OpenTF founders is that Terraform should continue to be open source because:");
            htmlPageBuilder.AppendLine("<ol>");
            htmlPageBuilder.Append("<li>HashiCorp benefited from 'free labor' that was tricked into contributing to their Open Source projects</li>");
            htmlPageBuilder.Append("<li>HashiCorp required potential contributors to sign a CLA, essentially ceeding their rights to their contributions under a promise to keep Terraform Open Source</li>");
            htmlPageBuilder.AppendLine("</ol>");

            htmlPageBuilder.AppendLine("Let's assume we take these claims at face value. Even if these things were true. Who would be impacted? How many people contributed to HashiCorp's projects that signed OpenTF? Will OpenTF actually be able to compete with HashiCorp in maintaining their forked version of Terraform? YOU DECIDE.");

            htmlPageBuilder.AppendLine($"<h2>Total Cosigners: {pageSummary.TotalCount}</h2>");
            htmlPageBuilder.AppendLine($"<h2>Active GitHub Users: {pageSummary.ActiveGitHubUsersCount}</h2>");
            htmlPageBuilder.AppendLine($"<h2>% Contributors Across ALL projects: {pctActualContributors.ToString("P")}</h2>");

            htmlPageBuilder.AppendLine($"<h2>Total Contributions by Cosigners Acrosss ALL projects: {allContributionsAcrossProjects}</h2>");
            htmlPageBuilder.AppendLine($"<h2>Total Contributions Acrosss ALL projects: {allPullRequestsAcrossProjects}</h2>");
            htmlPageBuilder.AppendLine($"<h2>% Contributed by Cosigners Acrosss ALL projects: {pctContributionsAcrossProjects.ToString("P")}</h2>");

            htmlPageBuilder.AppendLine("<ul>");
            foreach (var repo in repoSummaries)
            {
                double percentContributors = (double)repo.ContributorCount / (double)pageSummary.TotalCount;

                double percentOfPullRequests = (double)repo.PullRequestCount / (double)repo.TotalPullRequestCount;
 
                htmlPageBuilder.Append("<li>");
                htmlPageBuilder.Append($"{repo.Owner}/{repo.Repo}");

                htmlPageBuilder.AppendLine("<ul>");
                htmlPageBuilder.Append($"<li>Contributors: {repo.ContributorCount}</li>");
                htmlPageBuilder.Append($"<ul><li>{percentContributors.ToString("P")} of cosigners contributed to this repo.</li></ul>");
                htmlPageBuilder.Append($"<li>Pull Requests: {repo.PullRequestCount}</li>");
                htmlPageBuilder.Append($"<ul><li>{percentOfPullRequests.ToString("P")} of ALL pull requests ({repo.TotalPullRequestCount}) contributed by the cosigners.</li></ul>");
                htmlPageBuilder.AppendLine("</ul>");

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

