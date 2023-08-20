using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IBulkRequestProcessor
	{
        Task ProcessPullRequestPageRequest(ProcessRepositoryPageRequest repoPageRequest);
        Task ProcessGitHubUserContributionRequest(ProcessGitHubUserProviderRequest userRequest);
    }
}