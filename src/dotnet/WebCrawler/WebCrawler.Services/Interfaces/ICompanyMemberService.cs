using System;
namespace WebCrawler.Services.Interfaces
{
	public interface ICompanyMemberService
	{
		Task<List<string>> GetCompanyMembers(string companyName);
	}
}

