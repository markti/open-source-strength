using System;
namespace GitHubCrawler.Model
{
    public class Cosigner
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public string EntityType { get; set; }
        public string SupportOffered { get; set; }
        public string GitHubUserName { get; set; }
    }
}