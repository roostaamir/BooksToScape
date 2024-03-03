using System.Text.RegularExpressions;
using BooksToScape.App.Common;
using BooksToScape.App.Services.Interfaces;
using HtmlAgilityPack;

namespace BooksToScape.App.Services;

/// <summary>
/// The purpose of this component is to be able to get a working copy of the root page to work offline
/// By downloading all the necessary dependencies of the page offline and modifying the links to them
/// correctly
/// </summary>
public class RootPageOnlyCrawler : IBooksToScrapeCrawler
{
    public async Task Crawl(string directoryToDownload)
    {
        if (!Directory.Exists(directoryToDownload))
        {
            //TODO check directory is valid
            Directory.CreateDirectory(directoryToDownload);
        }

        var client = new HttpClient();

        var responseString = await client.GetStringAsync(ApplicationConstants.RootUrl);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(responseString);

        var headLinks = htmlDocument.DocumentNode
            .Descendants("link")
            .Where(el => el.Attributes["href"] is not null)
            .Select(el => new Uri(el.Attributes["href"].Value, UriKind.RelativeOrAbsolute))
            .ToList();

        var images = htmlDocument.DocumentNode
            .Descendants("img")
            .Where(el => el.Attributes["src"] is not null)
            .Select(el => new Uri(el.Attributes["src"].Value, UriKind.RelativeOrAbsolute))
            .ToList();

        var scripts = htmlDocument.DocumentNode
            .Descendants("script")
            .Where(el => el.Attributes["src"] is not null)
            .Select(el => new Uri(el.Attributes["src"].Value, UriKind.RelativeOrAbsolute))
            .ToList();

        var allUrlsToDownload = headLinks
            .Concat(images)
            .Concat(scripts);

        foreach (var url in allUrlsToDownload)
        {
            if (!url.IsAbsoluteUri)
            {
                await DownloadLocallyAsync(client, new Uri(new Uri(ApplicationConstants.RootUrl), url), directoryToDownload);
            }
        }

        await using var writer = new StreamWriter(Path.Combine(directoryToDownload, "index.html"));
        htmlDocument.Save(writer);
    }

    private async Task DownloadLocallyAsync(HttpClient client, Uri inputUri, string directoryToDownload)
    {
        var response = await client.GetAsync(inputUri);

        if (response.IsSuccessStatusCode)
        {
            var localPath = Path.Combine(directoryToDownload, GetCleanLocalPath(inputUri));

            localPath = localPath.Replace('/', '\\');

            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            await using var fileStream = new FileStream(localPath, FileMode.Create);
            await response.Content.CopyToAsync(fileStream);

            if (Path.GetExtension(localPath).Equals(".css", StringComparison.InvariantCulture))
            {
                var cssText = await response.Content.ReadAsStringAsync();
                var urlsInsideCss = Regex.Matches(cssText, """url\(['"]?(.*?)['"]?\)""")
                    .Select(g => g.Groups[1].Value)
                    .ToList();

                foreach (var url in urlsInsideCss)
                {
                    await DownloadLocallyAsync(
                        client,
                        new Uri(inputUri, new Uri(url, UriKind.RelativeOrAbsolute)),
                        directoryToDownload);
                }
            }
        }

        //TODO do something with the exception
    }

    private static string GetCleanLocalPath(Uri inputUri)
    {
        var localPath = inputUri.LocalPath
            .TrimStart('/')
            .TrimEnd('/');

        var queryParameterStartIndex = localPath.LastIndexOf('?');

        return queryParameterStartIndex < 0
            ? localPath
            : localPath.Substring(0, queryParameterStartIndex);
    }
}
