using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubCrawler.Model;
using GitHubCrawler.Services;
using GitHubCrawler.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GitHubCrawler
{
    public class RepoReportGenerator
    {
        private readonly ILogger<RepoReportGenerator> _logger;
        private readonly IRepositoryReportGenerator _repoReportGenerator;
        private readonly IGitHubProjectService _gitHubProjectService;
        private readonly IWebPageGenerator _webPageGenerator;

        public RepoReportGenerator(ILogger<RepoReportGenerator> logger, IGitHubProjectService gitHubProjectService, IRepositoryReportGenerator pageProcessor, IWebPageGenerator webPageGenerator)
        {
            _logger = logger;
            _repoReportGenerator = pageProcessor;
            _gitHubProjectService = gitHubProjectService;
            _webPageGenerator = webPageGenerator;
        }

        [FunctionName("RepoReportGenerator")]
        public async Task GenerateReports([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer)
        {
            var projectList = _gitHubProjectService.GetProjects();

            var repoSummaryList = new List<RepositorySummary>();

            foreach(var project in projectList)
            {
                _logger.LogInformation($"Generating report for project {project.Owner}/{project.Repo}");

                var report = await _repoReportGenerator.GetUserContributionsAsync(project.Owner, project.Repo);

                repoSummaryList.Add(report);
                await _repoReportGenerator.SaveReportAsync(report);

            }
            var htmlResult = await _webPageGenerator.GenerateHtmlAsync(repoSummaryList);
            await _webPageGenerator.SaveAsync(htmlResult);
        }
    }
}