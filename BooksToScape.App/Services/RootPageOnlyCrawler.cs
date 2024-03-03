using System.Text.RegularExpressions;
using BooksToScape.App.Common;
using BooksToScape.App.Services.Interfaces;
using BooksToScape.App.Utils;
using HtmlAgilityPack;

namespace BooksToScape.App.Services;

/// <summary>
/// The purpose of this component is to be able to get a working copy of the root page to work offline
/// By downloading all the necessary dependencies of the page offline and modifying the links to them
/// correctly
/// </summary>
public class RootPageOnlyCrawler : IBooksToScrapeCrawler
{
    private readonly HttpClient _client;
    private readonly IResourceCrawler _resourceCrawler;

    public RootPageOnlyCrawler(HttpClient client, IResourceCrawler resourceCrawler)
    {
        _client = client;
        _resourceCrawler = resourceCrawler;
    }

    public Task CrawlAsync(string url, string rootDownloadDirectory)
    {
        if (!url.TrimEnd('/').Equals(ApplicationConstants.RootUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The crawler only supports crawling the root page of books.To.Scrape.com");
        }

        if (!Directory.Exists(rootDownloadDirectory))
        {
            //TODO check directory is valid
            Directory.CreateDirectory(rootDownloadDirectory);
        }

        return CrawlInternalAsync(rootDownloadDirectory);
    }

    private async Task CrawlInternalAsync(string rootDownloadDirectory)
    {
        var responseString = await _client.GetStringAsync(ApplicationConstants.RootUrl);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(responseString);

        await _resourceCrawler.DownloadLocalResourcesAsync(htmlDocument.GetAllResourceUris(), rootDownloadDirectory);

        await using var writer = new StreamWriter(Path.Combine(rootDownloadDirectory, "index.html"));
        htmlDocument.Save(writer);
    }
}
