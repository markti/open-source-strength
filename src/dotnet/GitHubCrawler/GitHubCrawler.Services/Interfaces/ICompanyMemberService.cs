using System;
namespace GitHubCrawler.Services.Interfaces
{
    public interface ICompanyMemberService
    {
        Task<List<string>> GetCompanyMembers(string companyName);
    }
}