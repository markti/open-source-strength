using System;
namespace GitHubCrawler.Model
{
	public class ProcessRepositoryPageRequest
    {
        public string Owner { get; set; }
        public string Repo { get; set; }
        public string PageNumber { get; set; }
    }
}