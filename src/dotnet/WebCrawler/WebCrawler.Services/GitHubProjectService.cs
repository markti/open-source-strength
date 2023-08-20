using System;
using WebCrawler.Model;
using WebCrawler.Services.Interfaces;

namespace WebCrawler.Services
{
    public class GitHubProjectService : IGitHubProjectService
    {
        public List<GitHubProject> GetProjects()
        {
            var list = new List<GitHubProject>();

            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "terraform"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "terraform-provider-aws"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "terraform-provider-azurerm"
            });

            return list;
        }
    }
}