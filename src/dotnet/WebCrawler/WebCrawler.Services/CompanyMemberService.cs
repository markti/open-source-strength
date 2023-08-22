using System;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebCrawler.Model;
using WebCrawler.Services.Interfaces;

namespace WebCrawler.Services
{
	public class CompanyMemberService : ICompanyMemberService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<CompanyMemberService> _logger;
        private readonly BlobConfig _blobConfig;

        public CompanyMemberService(
            ILogger<CompanyMemberService> logger,
            TelemetryConfiguration telemetryConfiguration,
            BlobConfig blobConfig)
        {
            _logger = logger;
            _telemetryClient = new TelemetryClient(telemetryConfiguration);
            _blobConfig = blobConfig;
        }

        public async Task<List<string>> GetCompanyMembers(string companyName)
        {
            var memberList = new List<string>();
            var blobServiceClient = new BlobServiceClient(_blobConfig.ConnectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(BlobContainerNames.COMPANIES);
            var blobName = $"{companyName.ToLower()}.txt";
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            var blobExistsResult = await blobClient.ExistsAsync();

            if(blobExistsResult.Value)
            {

                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                string blobContents = downloadResult.Content.ToString();
                var splitMembers = blobContents.Split('\n');

                foreach(var item in splitMembers)
                {
                    if(!string.IsNullOrEmpty(item))
                    {
                        memberList.Add(item);
                    }
                }
            }

            return memberList;
        }
    }
}