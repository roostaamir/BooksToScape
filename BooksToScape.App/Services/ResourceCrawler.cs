using System.Text.RegularExpressions;
using BooksToScape.App.Common;
using BooksToScape.App.Services.Interfaces;
using BooksToScape.App.Utils;

namespace BooksToScape.App.Services;

public class ResourceCrawler : IResourceCrawler
{
    private readonly HttpClient _client;

    public ResourceCrawler(HttpClient client)
    {
        _client = client;
    }

    public async Task DownloadLocalResourcesAsync(List<Uri> resourceUris, string rootDownloadDirectory)
    {
        foreach (var resourceUrl in resourceUris)
        {
            if (!resourceUrl.IsAbsoluteUri)
            {
                await DownloadLocalResourcesInternalAsync(
                    new Uri(new Uri(ApplicationConstants.RootUrl), resourceUrl),
                    rootDownloadDirectory);
            }
        }
    }

    private async Task DownloadLocalResourcesInternalAsync(Uri inputUri, string rootDownloadDirectory)
    {
        var localPath = Path.Combine(rootDownloadDirectory, GetCleanLocalPath(inputUri));
        Directory.CreateDirectory(Path.GetDirectoryName(localPath));

        var response = await _client.GetAsync(inputUri);

        if (!response.IsSuccessStatusCode)
        {
            //TODO do something with the exception
            return;
        }

        if (Path.GetExtension(localPath).Equals(".css", StringComparison.InvariantCulture))
        {
            var cssText = await response.Content.ReadAsStringAsync();

            var urlsInsideCss = CssHelpers.GetAllInternalUrls(cssText);

            foreach (var url in urlsInsideCss)
            {
                await DownloadLocalResourcesInternalAsync(
                    new Uri(inputUri, new Uri(url, UriKind.RelativeOrAbsolute)),
                    rootDownloadDirectory);
            }

            cssText = CssHelpers.GetWithTrimmedQueryStringsFromInternalUrls(cssText);

            await File.WriteAllTextAsync(localPath, cssText);
            return;
        }

        await using var fileStream = new FileStream(localPath, FileMode.Create);
        await response.Content.CopyToAsync(fileStream);
    }

    private static string GetCleanLocalPath(Uri inputUri)
    {
        var localPath = inputUri.LocalPath
            .TrimStart('/')
            .TrimEnd('/')
            .Replace('/', '\\');

        var queryParameterStartIndex = localPath.LastIndexOf('?');

        return queryParameterStartIndex < 0
            ? localPath
            : localPath.Substring(0, queryParameterStartIndex);
    }
}