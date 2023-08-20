using System;
using WebCrawler.Model;

namespace WebCrawler.Services.Interfaces
{
	public interface IFanoutRequestProcessor
	{
        Task ProcessGitHubUserAsync(string username);
        Task ProcessGitHubUserContributionAsync(ProcessGitHubUserProviderRequest githubUserContributionRequest);
    }
}