using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
    public interface IGitHubProjectService
    {
        List<GitHubProject> GetProjects();
    }
}

