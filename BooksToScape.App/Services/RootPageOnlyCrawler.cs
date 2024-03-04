using BooksToScape.App.Common;
using BooksToScape.App.Errors;
using BooksToScape.App.Services.Interfaces;
using BooksToScape.App.Utils;
using FluentResults;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<RootPageOnlyCrawler> _logger;

    public RootPageOnlyCrawler(HttpClient client, IResourceCrawler resourceCrawler, ILogger<RootPageOnlyCrawler> logger)
    {
        _client = client;
        _resourceCrawler = resourceCrawler;
        _logger = logger;
    }

    public async Task<Result> CrawlAsync(string rootDownloadDirectory)
    {
        try
        {
            if (!Directory.Exists(rootDownloadDirectory))
            {
                Directory.CreateDirectory(rootDownloadDirectory);
            }
        }
        catch (Exception exception)
        {
            return Result.Fail(new DirectoryNotValidError(rootDownloadDirectory)
                .CausedBy(exception));
        }

        return await CrawlInternalAsync(rootDownloadDirectory);
    }

    private async Task<Result> CrawlInternalAsync(string rootDownloadDirectory)
    {
        var uriToCrawl = new Uri(ApplicationConstants.RootPage);

        try
        {
            var htmlDocument = await GetHtmlDocument(uriToCrawl);

            var resourceUris = htmlDocument.GetRelativeResourceUris()
                .Select(uri => new Uri(uriToCrawl, uri))
                .ToList();

            var crawlResourcesResult =
                await _resourceCrawler.DownloadLocalResourcesAsync(resourceUris, rootDownloadDirectory);

            await using var writer = new StreamWriter(Path.Combine(rootDownloadDirectory, "index.html"));
            htmlDocument.Save(writer);

            return crawlResourcesResult;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Exception occured while processing page url {PageUrl}",
                uriToCrawl.ToString());

            return Result.Fail(new Error($"Failed to process [{uriToCrawl.ToString()}]")
                .CausedBy(exception));
        }
    }

    private async Task<HtmlDocument> GetHtmlDocument(Uri uriToCrawl)
    {
        var responseString = await _client.GetStringAsync(uriToCrawl);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(responseString);

        return htmlDocument;
    }
}
