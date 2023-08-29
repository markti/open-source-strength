using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IRepositoryReportGenerator
    {
        Task<RepositorySummary> GetUserContributionsAsync(string repositoryOwner, string repositoryName);
        Task SaveReportAsync(RepositorySummary report);
    }
}