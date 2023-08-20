using System;
using WebCrawler.Model;

namespace WebCrawler.Services.Interfaces
{
	public interface IGitHubProjectService
	{
		List<GitHubProject> GetProjects();
	}
}

