using GitHubCrawler.Model;
using GitHubCrawler.Services.Interfaces;
using Octokit;

namespace GitHubCrawler.Services;

public class GitHubQueryService : IGitHubQueryService
{
    private readonly string _patToken;

    public GitHubQueryService(string patToken)
    {
        _patToken = patToken;
    }

    public async Task<List<PullRequestSummary>> GetPullRequestHistory(string ownerName, string repoName, int startPage)
    {
        var client = new GitHubClient(new ProductHeaderValue("OpenSourceStrength"));
        client.Credentials = new Credentials(_patToken);
        // Fetch commits
        var request = new PullRequestRequest
        {
            State = ItemStateFilter.Closed
        };
        var apiOptions = new ApiOptions
        {
            PageCount = 1,
            PageSize = 100,
            StartPage = startPage
        };

        // Get the first page of commits
        var pullRequests = await client.PullRequest.GetAllForRepository(ownerName, repoName, request, apiOptions);

        var mergedPRsByUser = pullRequests.Where(pr => pr.Merged).ToList();

        var prHistory = new List<PullRequestSummary>();
        foreach (var pr in mergedPRsByUser)
        {
            var prSummary = new PullRequestSummary()
            {
                Id = pr.Id,
                Number = pr.Number,
                MergedAt = pr.MergedAt.Value.UtcDateTime,
                UserName = pr.User.Login
            };
            prHistory.Add(prSummary);
        }

        return prHistory;
    }
}