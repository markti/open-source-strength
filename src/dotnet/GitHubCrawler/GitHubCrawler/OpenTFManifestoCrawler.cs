using System;
using System.Threading.Tasks;
using GitHubCrawler.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GitHubCrawler
{
    public class OpenTFManifestoCrawler
    {
        private readonly ILogger<OpenTFManifestoCrawler> _logger;
        private readonly IPageProcessor _pageProcessor;

        public OpenTFManifestoCrawler(ILogger<OpenTFManifestoCrawler> logger, IPageProcessor pageProcessor)
        {
            _logger = logger;
            _pageProcessor = pageProcessor;
        }

        [FunctionName("opentf-crawler")]
        public async Task Run([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            var results = await _pageProcessor.ProcessPageAsync();
        }
    }
}