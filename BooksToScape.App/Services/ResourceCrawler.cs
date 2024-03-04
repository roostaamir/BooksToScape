using BooksToScape.App.Services.Interfaces;
using BooksToScape.App.Utils;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace BooksToScape.App.Services;

public class ResourceCrawler : IResourceCrawler
{
    private readonly HttpClient _client;
    private readonly ILogger<ResourceCrawler> _logger;

    public ResourceCrawler(HttpClient client, ILogger<ResourceCrawler> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<Result> DownloadLocalResourcesAsync(List<Uri> resourceUris, string rootDownloadDirectory)
    {
        var results = new List<Result>();

        foreach (var resourceUrl in resourceUris)
        {
            var result = await DownloadLocalResourcesInternalAsync(
                resourceUrl,
                rootDownloadDirectory);

            results.Add(result);
        }

        return results.Merge();
    }

    private async Task<Result> DownloadLocalResourcesInternalAsync(Uri inputUri, string rootDownloadDirectory)
    {
        try
        {
            var response = await _client.GetAsync(inputUri);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Request to get {ResourceUrl} failed with status code {FailedStatusCode}",
                    inputUri.ToString(),
                    response.StatusCode);

                return Result.Fail(
                    $"Request to get {inputUri.ToString()} failed with status code {response.StatusCode.ToString()}");
            }

            var localPath = inputUri.GetLocalPathAndEnsureCreated(rootDownloadDirectory);

            if (!Path.GetExtension(localPath).Equals(".css", StringComparison.InvariantCulture))
            {
                await SaveResourceAsync(localPath, response);
                return Result.Ok();
            }

            var cssText = await response.Content.ReadAsStringAsync();

            var result = await CrawlRelativeResourcesInsideCssTextAsync(inputUri, rootDownloadDirectory, cssText);

            await SaveCssToFileAsync(cssText, localPath);

            return Result.Merge(result, Result.Ok());
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Exception occured while processing resource url {ResourceUrl}",
                inputUri.ToString());

            return Result.Fail(new Error($"Failed to process [{inputUri.ToString()}]")
                .CausedBy(exception));
        }
    }

    private static async Task SaveResourceAsync(string localPath, HttpResponseMessage response)
    {
        await using var fileStream = new FileStream(localPath, FileMode.Create);
        await response.Content.CopyToAsync(fileStream);
    }

    private static async Task SaveCssToFileAsync(string cssText, string localPath)
    {
        cssText = CssHelpers.GetWithTrimmedQueryStringsFromInternalUrls(cssText);
        await File.WriteAllTextAsync(localPath, cssText);
    }

    private async Task<Result> CrawlRelativeResourcesInsideCssTextAsync(
        Uri inputUri,
        string rootDownloadDirectory,
        string cssText)
    {
        var urisInsideCss = CssHelpers.GetAllInternalUrls(cssText)
            .Select(url => new Uri(inputUri, new Uri(url, UriKind.RelativeOrAbsolute)))
            .ToList();

        return await DownloadLocalResourcesAsync(urisInsideCss, rootDownloadDirectory);
    }
}
