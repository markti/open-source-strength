using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services
{
	public interface IGitHubQueryService
	{
        Task<List<PullRequestSummary>> GetPullRequestHistory(string ownerName, string repoName, int startPage);
    }
}