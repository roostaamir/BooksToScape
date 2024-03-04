using BooksToScape.App.Common;
using BooksToScape.App.Errors;
using BooksToScape.App.Services.Interfaces;
using BooksToScape.App.Utils;
using FluentResults;
using HtmlAgilityPack;

namespace BooksToScape.App.Services;

/// <summary>
/// This is a greedy implementation of the crawler and thread-safety should not be a concern
/// </summary>
public class DefaultCrawler : IBooksToScrapeCrawler
{
    private const int MaxDepth = 51;

    private readonly HttpClient _client;
    private readonly IResourceCrawler _resourceCrawler;

    private readonly List<Uri> _visitedUris;

    public DefaultCrawler(HttpClient client, IResourceCrawler resourceCrawler)
    {
        _client = client;
        _resourceCrawler = resourceCrawler;

        _visitedUris = new List<Uri>();
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

        return await CrawlInternalAsync(new Uri(ApplicationConstants.RootPage), rootDownloadDirectory, 0);
    }

    private async Task<Result> CrawlInternalAsync(Uri uriToCrawl, string rootDownloadDirectory, int level)
    {
        if (level++ > MaxDepth)
        {
            return Result.Ok();
        }

        var htmlDocument = await GetHtmlDocument(uriToCrawl);

        var crawlLinksResult = await CrawlUnvisitedRelativeUris(uriToCrawl, rootDownloadDirectory, level, htmlDocument);
        var crawlResourcesResult = await CrawlUnvisitedRelativeResources(uriToCrawl, rootDownloadDirectory, htmlDocument);

        await htmlDocument.SaveToFile(
            Path.Combine(rootDownloadDirectory, uriToCrawl.GetCleanedLocalDirectoryPath()));

        return Result.Merge(crawlLinksResult, crawlResourcesResult);
    }

    private async Task<Result> CrawlUnvisitedRelativeResources(Uri uriToCrawl, string rootDownloadDirectory,
        HtmlDocument htmlDocument)
    {
        var unvisitedResourceUris = htmlDocument.GetRelativeResourceUris()
            .Select(uri => new Uri(uriToCrawl, uri))
            .Except(_visitedUris)
            .ToList();

        _visitedUris.AddRange(unvisitedResourceUris);

        return await _resourceCrawler.DownloadLocalResourcesAsync(unvisitedResourceUris, rootDownloadDirectory);
    }

    private async Task<Result> CrawlUnvisitedRelativeUris(Uri uriToCrawl, string rootDownloadDirectory, int level,
        HtmlDocument htmlDocument)
    {
        var unvisitedLocalUris = htmlDocument.GetRelativeAnchorLinkUris()
            .Select(uri => new Uri(uriToCrawl, uri))
            .Except(_visitedUris)
            .ToList();

        _visitedUris.AddRange(unvisitedLocalUris);

        var results = new List<Result>();

        foreach (var uri in unvisitedLocalUris)
        {
            var result = await CrawlInternalAsync(uri, rootDownloadDirectory, level);
            results.Add(result);
        }

        return results.Merge();
    }

    private async Task<HtmlDocument> GetHtmlDocument(Uri uriToCrawl)
    {
        var responseString = await _client.GetStringAsync(uriToCrawl);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(responseString);

        return htmlDocument;
    }
}
