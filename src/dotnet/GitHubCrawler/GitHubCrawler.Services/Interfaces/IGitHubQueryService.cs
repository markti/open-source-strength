using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IGitHubQueryService
	{
        Task<List<PullRequestSummary>> GetPullRequestHistory(ProcessRepositoryPageRequest repoPageRequest);
    }
}