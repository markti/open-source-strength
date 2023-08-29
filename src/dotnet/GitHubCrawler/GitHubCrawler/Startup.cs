using GitHubCrawler.Services;
using GitHubCrawler.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using GitHubCrawler.Model;

[assembly: FunctionsStartup(typeof(GitHubCrawler.Startup))]

namespace GitHubCrawler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            var patToken = Environment.GetEnvironmentVariable("GITHUB_PAT_TOKEN");
            var gitHubQueryService = new GitHubQueryService(patToken);

            var queueConnectionString = Environment.GetEnvironmentVariable("QUEUE_CONNECTION_STRING");
            var queueConfig = new QueueConfig()
            {
                ConnectionString = queueConnectionString
            };
            var blobConnectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION_STRING");
            var blobConfig = new BlobConfig()
            {
                ConnectionString = blobConnectionString
            };

            // Register your services here
            builder.Services.AddSingleton<IGitHubQueryService>(gitHubQueryService);
            // Configuration for Queue
            builder.Services.AddSingleton<QueueConfig>(queueConfig);
            builder.Services.AddSingleton<BlobConfig>(blobConfig);
            builder.Services.AddSingleton<ICompanyMemberService, CompanyMemberService>();
            builder.Services.AddSingleton<IGitHubProjectService, GitHubProjectService>();
            builder.Services.AddSingleton<IBulkRequestProcessor, BulkRequestProcessor>();
            builder.Services.AddSingleton<IFanoutRequestProcessor, FanoutRequestProcessor>();
            builder.Services.AddSingleton<IPageProcessor, PageProcessor>();
        }
    }
}