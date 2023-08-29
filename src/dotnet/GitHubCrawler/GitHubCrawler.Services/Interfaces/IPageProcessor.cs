using System;
using GitHubCrawler.Model;

namespace GitHubCrawler.Services.Interfaces
{
    public interface IPageProcessor
    {
        Task<CosignerSummary> ProcessPageAsync();
        Task<CosignerSummary> GetLatestAsync();
        Task<CosignerSummary> GetFromBlobAsync(string blobName);
    }
}