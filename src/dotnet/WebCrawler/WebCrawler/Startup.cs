using WebCrawler.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using WebCrawler.Model;
using WebCrawler.Services.Interfaces;

[assembly: FunctionsStartup(typeof(WebCrawler.Startup))]

namespace WebCrawler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights();
            });
            builder.Services.AddApplicationInsightsTelemetry();

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
            // Configuration for Queue
            builder.Services.AddSingleton<QueueConfig>(queueConfig);
            builder.Services.AddSingleton<BlobConfig>(blobConfig);
            builder.Services.AddSingleton<IGitHubProjectService, GitHubProjectService>();
            builder.Services.AddSingleton<IFanoutRequestProcessor, FanoutRequestProcessor>();
            builder.Services.AddSingleton<IPageProcessor, PageProcessor>();
        }
    }
}