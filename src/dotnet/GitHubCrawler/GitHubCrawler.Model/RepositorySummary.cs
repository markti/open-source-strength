using System;
namespace GitHubCrawler.Model
{
	public class RepositorySummary
	{
		public string Owner { get; set; }
		public string Repo { get; set; }
		public int ContributorCount { get; set; }
		public int PullRequestCount { get; set; }

		public Dictionary<string, int> Contributors { get; set; }
	}
}