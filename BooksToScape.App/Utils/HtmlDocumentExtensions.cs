using HtmlAgilityPack;

namespace BooksToScape.App.Utils;

public static class HtmlDocumentExtensions
{
    public static List<Uri> GetRelativeResourceUris(this HtmlDocument htmlDocument)
    {
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

        return headLinks
            .Concat(images)
            .Concat(scripts)
            .Where(uri => !uri.IsAbsoluteUri)
            .ToList();
    }

    public static List<Uri> GetRelativeAnchorLinkUris(this HtmlDocument htmlDocument)
    {
        return htmlDocument.DocumentNode
            .Descendants("a")
            .Where(el => el.Attributes["href"] is not null)
            .Select(el => new Uri(el.Attributes["href"].Value, UriKind.RelativeOrAbsolute))
            .Where(uri => !uri.IsAbsoluteUri)
            .ToList();
    }

    public static async Task SaveToFile(this HtmlDocument htmlDocument, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        await using var writer = new StreamWriter(filePath);
        htmlDocument.Save(writer);
    }
}