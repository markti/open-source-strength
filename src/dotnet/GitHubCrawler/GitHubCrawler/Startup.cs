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
            builder.Services.AddScoped<IBulkRequestProcessor, BulkRequestProcessor>();
            builder.Services.AddScoped<IFanoutRequestProcessor, FanoutRequestProcessor>();
        }
    }
}