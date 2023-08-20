using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GitHubCrawler
{
    public class GitHubRequestProcessor
    {
        [FunctionName("pull-request-page")]
        public void Run([QueueTrigger("pull-request-page", Connection = "STORAGE_CONNECTION_STRING")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}

