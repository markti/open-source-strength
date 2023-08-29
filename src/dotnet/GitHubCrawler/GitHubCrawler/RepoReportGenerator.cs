using System;
using System.Threading.Tasks;
using GitHubCrawler.Services.Interfaces;
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

        public RepoReportGenerator(ILogger<RepoReportGenerator> logger, IGitHubProjectService gitHubProjectService, IRepositoryReportGenerator pageProcessor)
        {
            _logger = logger;
            _repoReportGenerator = pageProcessor;
            _gitHubProjectService = gitHubProjectService;
        }

        [FunctionName("RepoReportGenerator")]
        public async Task GenerateReports([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer)
        {
            var projectList = _gitHubProjectService.GetProjects();

            foreach(var project in projectList)
            {
                _logger.LogInformation($"Generating report for project {project.Owner}/{project.Repo}");

                var report = await _repoReportGenerator.GetUserContributionsAsync(project.Owner, project.Repo);

                await _repoReportGenerator.SaveReportAsync(report);
            }
        }
    }
}