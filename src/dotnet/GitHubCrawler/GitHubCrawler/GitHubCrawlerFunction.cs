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
using GitHubCrawler.Services.Interfaces;

namespace GitHubCrawler
{
    public class GitHubCrawlerFunction
    {
        private readonly ILogger<GitHubCrawlerFunction> _logger;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;

        public GitHubCrawlerFunction(ILogger<GitHubCrawlerFunction> log, IFanoutRequestProcessor fanoutRequestProcessor)
        {
            _logger = log;
            _fanoutRequestProcessor = fanoutRequestProcessor;
        }

        [FunctionName("github-repo-pull-request-history")]
        public async Task<IActionResult> GenerateGitHubRepoReport(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github/pull-request/history")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<ProcessRepositoryRequest>(requestBody);

            var pageRequest = new ProcessRepositoryPageRequest()
            {
                Owner = request.Owner,
                Repo = request.Repo,
                PageNumber = 1
            };

            await _fanoutRequestProcessor.ProcessRepoPullRequestHistoryAsync(pageRequest);

            return new OkResult();
        }
    }
}