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
using GitHubCrawler.Model;

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

        [FunctionName("github-repo-report-generate")]
        public async Task<IActionResult> GenerateGitHubRepoReport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github")] HttpRequest req)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<ProcessRepositoryRequest>(requestBody);

            string responseMessage = $"Request to process {request.Owner}/{request.Repo}";

            _logger.LogInformation(responseMessage);

            return new OkObjectResult(responseMessage);
        }
    }
}