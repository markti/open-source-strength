using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using WebCrawler.Services.Interfaces;

namespace WebCrawler
{
    public class DailyCrawler
    {
        private readonly ILogger<DailyCrawler> _logger;
        private readonly IPageProcessor _pageProcessor;

        public DailyCrawler(ILogger<DailyCrawler> logger, IPageProcessor pageProcessor)
        {
            _logger = logger;
            _pageProcessor = pageProcessor;
        }

        [FunctionName("daily-crawler")]
        public async Task Run([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer)
        {
            var results = await _pageProcessor.ProcessPageAsync();
        }
    }
}