using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
	public interface IBulkRequestProcessor
	{
        Task ProcessPullRequestPageRequest(ProcessRepositoryRequest processRepoRequest);
    }
}