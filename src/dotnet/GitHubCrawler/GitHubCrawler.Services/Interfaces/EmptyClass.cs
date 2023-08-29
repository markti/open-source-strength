using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IWebPageGenerator
    {
        Task<string> GenerateHtmlAsync(List<RepositorySummary> repoSummaries);
    }
}