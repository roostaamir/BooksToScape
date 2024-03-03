using HtmlAgilityPack;

namespace BooksToScape.App.Utils;

public static class HtmlDocumentExtensions
{
    public static List<Uri> GetAllResourceUris(this HtmlDocument htmlDocument)
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
            .ToList();
    }
}