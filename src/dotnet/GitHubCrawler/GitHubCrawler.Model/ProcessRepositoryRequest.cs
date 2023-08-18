using System;
namespace GitHubCrawler.Model
{
	public class ProcessRepositoryRequest
	{
		public string Owner { get; set; }
		public string Repo { get; set; }
	}
}