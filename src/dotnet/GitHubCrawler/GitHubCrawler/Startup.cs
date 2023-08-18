using System;
using GitHubCrawler.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GitHubCrawler.Startup))]

namespace GitHubCrawler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var patToken = Environment.GetEnvironmentVariable("GITHUB_PAT_TOKEN");
            var gitHubQueryService = new GitHubQueryService(patToken);

            // Register your services here
            builder.Services.AddSingleton<IGitHubQueryService>(gitHubQueryService);
        }
    }
}