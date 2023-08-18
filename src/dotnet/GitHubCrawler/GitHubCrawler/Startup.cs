using System;
using GitHubCrawler.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace GitHubCrawler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register your services here
            builder.Services.AddSingleton<IGitHubQueryService, GitHubQueryService>();
        }
    }
}