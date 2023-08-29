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
using System.Collections.Generic;

namespace GitHubCrawler
{
    public class GitHubCrawlerFunction
    {
        private readonly ILogger<GitHubCrawlerFunction> _logger;
        private readonly IFanoutRequestProcessor _fanoutRequestProcessor;
        private readonly IBulkRequestProcessor _bulkRequestProcessor;
        private readonly IGitHubProjectService _gitHubProjectService;

        public GitHubCrawlerFunction(ILogger<GitHubCrawlerFunction> log, IGitHubProjectService gitHubProjectService, IFanoutRequestProcessor fanoutRequestProcessor, IBulkRequestProcessor bulkRequestProcessor)
        {
            _logger = log;
            _fanoutRequestProcessor = fanoutRequestProcessor;
            _bulkRequestProcessor = bulkRequestProcessor;
            _gitHubProjectService = gitHubProjectService;
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


        [FunctionName("github-repo-pull-request-summary")]
        public async Task<IActionResult> GeneratePullRequestSummary(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github/pull-request/summary")] HttpRequest req)
        {

            var projectList = _gitHubProjectService.GetProjects();

            var repoSummaryList = new List<RepositorySummary>();

            foreach (var project in projectList)
            {
                _logger.LogInformation($"Generating report for project {project.Owner}/{project.Repo}");

                var totalCount = await _bulkRequestProcessor.CalculatePullRequestCountAsync(project.Owner, project.Repo);

                var containerName = BlobContainerNames.PULL_REQUESTS;
                var blobName = $"{project.Owner}/${project.Repo}/total.txt";
                await _bulkRequestProcessor.SaveAsync(containerName, blobName, totalCount.ToString());
            }

            return new OkResult();
        }
    }
}