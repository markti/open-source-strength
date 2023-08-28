using System;
namespace GitHubCrawler.Model
{
    public class CosignerSummary
    {
        public int CompanyCount { get; set; }
        public int ProjectCount { get; set; }
        public int IndividualCount { get; set; }
        public int TotalCount { get; set; }

        public int ActiveGitHubUsersCount { get; set; }

        public List<Cosigner> Cosigners { get; set; }
    }
}