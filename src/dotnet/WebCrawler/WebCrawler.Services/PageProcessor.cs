using HtmlAgilityPack;
using System;
using Azure.Storage.Blobs;
using System.Text.RegularExpressions;
using WebCrawler.Model;
using WebCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebCrawler.Services;
public class PageProcessor : IPageProcessor
{
    private readonly TelemetryClient _telemetryClient;
    private readonly ILogger<PageProcessor> _logger;
    private const string GITHUB_USER_REGEX = @"https://github\.com/([^/]+)";
    private const string OPEN_TF_URL = "https://raw.githubusercontent.com/opentffoundation/manifesto/main/index.html";
    private readonly BlobConfig _blobConfig;
    private readonly IFanoutRequestProcessor _fanoutRequestProcessor;

    public PageProcessor(
            ILogger<PageProcessor> logger,
            TelemetryConfiguration telemetryConfiguration,
            BlobConfig blobConfig,
            IFanoutRequestProcessor fanoutRequestProcessor)
    {
        _logger = logger;
        _telemetryClient = new TelemetryClient(telemetryConfiguration);
        _blobConfig = blobConfig;
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
                var cosignerCell = cells.ElementAt(0);
                var cosignerAnchor = cosignerCell.SelectSingleNode("./a");
                newCosigner.Url = cosignerAnchor.Attributes["href"].Value;
                newCosigner.Name = cosignerAnchor.InnerText;
                newCosigner.EntityType = cells.ElementAt(1).InnerText;
                newCosigner.SupportOffered = cells.ElementAt(2).InnerText;

                Match match = Regex.Match(newCosigner.Url, GITHUB_USER_REGEX);
                if (match.Success)
                {
                    string username = match.Groups[1].Value;
                    newCosigner.GitHubUserName = username;

                    await _fanoutRequestProcessor.ProcessGitHubUserAsync(username);
                }

                cosigners.Add(newCosigner);
            }

            summary.CompanyCount = cosigners.Where(f => f.EntityType == "Company").Count();
            summary.ProjectCount = cosigners.Where(f => f.EntityType == "Project").Count();
            summary.IndividualCount = cosigners.Where(f => f.EntityType == "Individual").Count();
            summary.ActiveGitHubUsersCount = cosigners.Where(f => !string.IsNullOrEmpty(f.GitHubUserName)).Count();

            summary.Cosigners = cosigners;

            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerNames.PULL_REQUESTS);

            DateTime utcTimestamp = DateTime.UtcNow;

            var blobName = "open_tf/" + utcTimestamp.ToString("dd_MM_yyyy") + ".json";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var pageData = JsonConvert.SerializeObject(summary);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(pageData));
            await blobClient.UploadAsync(stream, overwrite: true);

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
}