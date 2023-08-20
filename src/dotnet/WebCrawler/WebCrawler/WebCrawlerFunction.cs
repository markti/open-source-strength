using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebCrawler.Services.Interfaces;

namespace WebCrawler
{
    public class WebCrawlerFunction
    {
        private readonly ILogger<WebCrawlerFunction> _logger;
        private readonly IPageProcessor _pageProcessor;

        public WebCrawlerFunction(ILogger<WebCrawlerFunction> logger, IPageProcessor pageProcessor)
        {
            _logger = logger;
            _pageProcessor = pageProcessor;
        }

        [FunctionName("web-crawler")]
        public async Task<IActionResult> CrawlPage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "web-crawler-start")] HttpRequest req)
        {
            var results = await _pageProcessor.ProcessPageAsync();

            return new OkObjectResult(results);
        }
    }
}