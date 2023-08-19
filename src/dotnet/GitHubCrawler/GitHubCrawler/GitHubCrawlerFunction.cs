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
        private readonly ILogger<GitHubCrawlerFunction> _logger;
        private readonly IGitHubQueryService _gitHubQueryService;

        public GitHubCrawlerFunction(ILogger<GitHubCrawlerFunction> log, IGitHubQueryService myService)
        {
            _logger = log;
            _gitHubQueryService = myService;
        }

        [FunctionName("GitHubCrawler")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github/{owner}/{repo}")] HttpRequest req, string owner, string repo)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request.{owner}/{repo}");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string responseMessage = "Hello Open Source Strength!";

            return new OkObjectResult(responseMessage);
        }
    }
}