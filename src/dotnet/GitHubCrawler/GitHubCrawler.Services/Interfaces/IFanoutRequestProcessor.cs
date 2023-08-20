using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IFanoutRequestProcessor
    {
        Task ProcessRepoPullRequestHistoryAsync(ProcessRepositoryPageRequest processRepoRequest);
    }
}