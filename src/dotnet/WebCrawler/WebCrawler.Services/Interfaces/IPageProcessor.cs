using System;
using WebCrawler.Model;

namespace WebCrawler.Services.Interfaces
{
	public interface IPageProcessor
	{
        Task<CosignerSummary> ProcessPageAsync();
    }
}