using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GitHubCrawler.Services;

namespace GitHubCrawler
{
    public class GitHubCrawlerFunction
    {
        private readonly IGitHubQueryService _gitHubQueryService;

        public GitHubCrawlerFunction(IGitHubQueryService myService)
        {
            _gitHubQueryService = myService;
        }

        [FunctionName("GitHubCrawler")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string responseMessage = "Hello Open Source Strength!";

            return new OkObjectResult(responseMessage);
        }
    }
}