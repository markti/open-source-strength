using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IFanoutRequestProcessor
    {
        Task ProcessRepoPullRequestHistoryAsync(ProcessRepositoryPageRequest repoPageRequest);
        Task ProcessGitHubUserAsync(string username);
        Task ProcessGitHubUserContributionAsync(ProcessGitHubUserProviderRequest githubUserContributionRequest);
    }
}