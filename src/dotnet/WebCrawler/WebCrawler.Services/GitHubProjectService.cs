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
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "vagrant"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "packer"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "waypoint"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "boundary"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "consul"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "nomad"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "vault"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "vault-secrets-operator"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "vault-csi-provider"
            });
            list.Add(new GitHubProject
            {
                Owner = "hashicorp",
                Repo = "go-kms-wrapping"
            });

            return list;
        }
    }
}