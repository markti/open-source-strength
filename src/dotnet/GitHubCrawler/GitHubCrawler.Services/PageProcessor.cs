﻿using System;
using Azure.Storage.Blobs;
using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlAgilityPack;
using Azure.Storage.Blobs.Models;

namespace GitHubCrawler.Services
{
    public class PageProcessor : IPageProcessor
    {
        private const string GITHUB_USER_REGEX = @"https://github\.com/([^/]+)";
        private const string OPEN_TF_URL = "https://raw.githubusercontent.com/opentffoundation/manifesto/main/index.html";

        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<PageProcessor> _logger;
        private readonly BlobConfig _blobConfig;
        private readonly ICompanyMemberService _companyMemberService;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;

        public PageProcessor(
                ILogger<PageProcessor> logger,
                TelemetryConfiguration telemetryConfiguration,
                BlobConfig blobConfig,
                ICompanyMemberService companyMemberService,
                IFanoutRequestProcessor fanoutRequestProcessor)
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _blobConfig = blobConfig;
            _companyMemberService = companyMemberService;
            _fanoutRequestProcessor = fanoutRequestProcessor;
        }

        public async Task<CosignerSummary> ProcessPageAsync()
        {
            CosignerSummary summary = new CosignerSummary();

            List<Cosigner> cosigners = new List<Cosigner>();
            var httpClient = new HttpClient();

            string htmlContent = await httpClient.GetStringAsync(OPEN_TF_URL);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Extract the table with the specified class.
            var tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='co-signed']");

            if (tableNode != null)
            {
                var rows = tableNode.SelectNodes("./tbody/tr").ToList();
                foreach (var row in rows)
                {
                    var newCosigner = new Cosigner();

                    var cells = row.SelectNodes("./td").ToList();
                    _logger.LogInformation(row.InnerHtml);
                    var cosignerCell = cells.ElementAt(0);
                    var cosignerAnchor = cosignerCell.SelectSingleNode("./a");
                    if(cosignerAnchor != null)
                    {
                        if(cosignerAnchor.Attributes.Contains("href"))
                        {
                            newCosigner.Url = cosignerAnchor.Attributes["href"].Value;
                            _logger.LogWarning("Anchor does not container an 'href' attribute!");
                        }
                        newCosigner.Name = cosignerAnchor.InnerText;
                    }
                    else
                    {
                        _logger.LogWarning("No Anchor Tag");
                    }
                    if(newCosigner.Name == "Fabio Pasetti - CloudFire")
                    {
                        _logger.LogWarning("Get Ready");
                    }
                    newCosigner.EntityType = cells.ElementAt(1).InnerText;
                    newCosigner.SupportOffered = cells.ElementAt(2).InnerText;

                    if(!string.IsNullOrEmpty(newCosigner.Url))
                    {
                        Match match = Regex.Match(newCosigner.Url, GITHUB_USER_REGEX);
                        if (match.Success)
                        {
                            string username = match.Groups[1].Value;
                            newCosigner.GitHubUserName = username;

                            await _fanoutRequestProcessor.ProcessGitHubUserAsync(username);
                        }
                    }

                    cosigners.Add(newCosigner);
                }

                var allCompanies = cosigners.Where(f => f.EntityType == "Company").ToList();
                foreach (var company in allCompanies)
                {
                    var companyMembers = await _companyMemberService.GetCompanyMembers(company.Name);
                    foreach (var companyMember in companyMembers)
                    {
                        var newCosigner = new Cosigner();
                        newCosigner.Name = $"{company.Name} Contributor";
                        newCosigner.EntityType = $"Company Contributor";
                        newCosigner.GitHubUserName = companyMember;
                        newCosigner.SupportOffered = $"{company.Name} Contributor";

                        await _fanoutRequestProcessor.ProcessGitHubUserAsync(companyMember);

                        cosigners.Add(newCosigner);
                    }
                }

                summary.CompanyContributorCount = cosigners.Where(f => f.EntityType == "Company Contributor").Count();
                summary.CompanyCount = cosigners.Where(f => f.EntityType == "Company").Count();
                summary.ProjectCount = cosigners.Where(f => f.EntityType == "Project").Count();
                summary.IndividualCount = cosigners.Where(f => f.EntityType == "Individual").Count();
                summary.ActiveGitHubUsersCount = cosigners.Where(f => !string.IsNullOrEmpty(f.GitHubUserName)).Count();

                summary.TotalCount = summary.CompanyContributorCount + summary.IndividualCount;

                summary.Cosigners = cosigners;

                DateTime utcTimestamp = DateTime.UtcNow;

                var containerName = BlobContainerNames.PAGES;
                var blobName = "open_tf/" + utcTimestamp.ToString("dd_MM_yyyy") + ".json";
                var pageData = JsonConvert.SerializeObject(summary);

                await SaveAsync(containerName, blobName, pageData);
                await SaveAsync(containerName, "open_tf/latest.json", pageData);

                var eventProperties = new Dictionary<string, string>();
                eventProperties.Add("companies", summary.CompanyCount.ToString());
                eventProperties.Add("projects", summary.ProjectCount.ToString());
                eventProperties.Add("individuals", summary.IndividualCount.ToString());
                eventProperties.Add("github-users", summary.ActiveGitHubUsersCount.ToString());

                _telemetryClient.TrackEvent("open-tf-refresh", eventProperties);
            }
            else
            {
                Console.WriteLine("Table with class 'co-signed' not found.");
            }
            return summary;
        }

        public async Task<CosignerSummary> GetLatestAsync()
        {
            return await GetFromBlobAsync("open_tf/latest.json");
        }
        public async Task<CosignerSummary> GetFromBlobAsync(string blobName)
        {
            var containerName = BlobContainerNames.PAGES;
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = blobContainerClient.GetBlobClient(blobName);

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            string blobContents = downloadResult.Content.ToString();
            var objectData = JsonConvert.DeserializeObject<CosignerSummary>(blobContents);

            return objectData;
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