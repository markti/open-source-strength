using System;
namespace GitHubCrawler.Model
{
    public class ProcessGitHubUserProviderRequest
    {
        public string UserName { get; set; }
        public string Owner { get; set; }
        public string Repo { get; set; }
    }
}

