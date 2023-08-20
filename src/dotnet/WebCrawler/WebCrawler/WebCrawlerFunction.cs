using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebCrawler
{
    public class WebCrawlerFunction
    {
        private readonly ILogger<WebCrawlerFunction> _logger;

        public WebCrawlerFunction(ILogger<WebCrawlerFunction> logger)
        {
            _logger = logger;
        }

        [FunctionName("page-crawler")]
        public async Task<IActionResult> CrawlPage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "page")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            return new OkResult();
        }
    }
}