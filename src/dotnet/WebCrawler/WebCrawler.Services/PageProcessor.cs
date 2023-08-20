using HtmlAgilityPack;
using System;
using WebCrawler.Model;
using WebCrawler.Services.Interfaces;

namespace WebCrawler.Services;
public class PageProcessor : IPageProcessor
{
    private const string targetUrl = "https://raw.githubusercontent.com/opentffoundation/manifesto/main/index.html";

    public async Task<CosignerSummary> ProcessPageAsync()
    {
        CosignerSummary summary = new CosignerSummary();

        List<Cosigner> cosigners = new List<Cosigner>();
        var httpClient = new HttpClient();

        string htmlContent = await httpClient.GetStringAsync(targetUrl);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        // Extract the table with the specified class.
        var tableNode = htmlDocument.DocumentNode.SelectSingleNode("//table[@class='co-signed']");

        if (tableNode != null)
        {
            var rows = tableNode.SelectNodes("./tbody/tr").ToList();
            foreach (var row in rows)
            {
                var newCosigner = new Cosigner();

                var cells = row.SelectNodes("./td").ToList();
                var cosignerCell = cells.ElementAt(0);
                var cosignerAnchor = cosignerCell.SelectSingleNode("./a");
                newCosigner.Url = cosignerAnchor.Attributes["href"].Value;
                newCosigner.Name = cosignerAnchor.InnerText;
                newCosigner.EntityType = cells.ElementAt(1).InnerText;
                newCosigner.SupportOffered = cells.ElementAt(2).InnerText;

                cosigners.Add(newCosigner);
            }

            summary.CompanyCount = cosigners.Where(f => f.EntityType == "Company").Count();
            summary.ProjectCount = cosigners.Where(f => f.EntityType == "Project").Count();
            summary.IndividualCount = cosigners.Where(f => f.EntityType == "Individual").Count();

            summary.Cosigners = cosigners;
        }
        else
        {
            Console.WriteLine("Table with class 'co-signed' not found.");
        }
        return summary;
    }
}

