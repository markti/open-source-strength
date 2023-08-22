using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IBulkRequestProcessor
	{
        Task ProcessPullRequestPageRequest(ProcessRepositoryPageRequest repoPageRequest);
        Task<int> ProcessGitHubUserContributionRequest(ProcessGitHubUserProviderRequest userRequest);
        Task<int> ProcessPullRequestTotal(string owner, string repo);
        Task SaveAsync(string containerName, string blobName, string blobContent);
    }
}