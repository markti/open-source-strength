namespace GitHubCrawler.Model;
public class PullRequestSummary
{
    public long Id { get; set; }
    public int Number { get; set; }
    public DateTime MergedAt { get; set; }
    public string UserName { get; set; }
}