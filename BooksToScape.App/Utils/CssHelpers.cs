using System.Text.RegularExpressions;

namespace BooksToScape.App.Utils;

public static partial class CssHelpers
{
    public static List<string> GetAllInternalUrls(string cssText)
    {
        return AllUrlValuesRegex().Matches(cssText)
            .Select(g => g.Groups[1].Value)
            .ToList();
    }

    public static string GetWithTrimmedQueryStringsFromInternalUrls(string cssText)
    {
        return TrimmedQueryStringsFromUrlsRegex().Replace(cssText, "$1$2");
    }

    [GeneratedRegex("""url\(['"]?(.*?)['"]?\)""")]
    private static partial Regex AllUrlValuesRegex();

    [GeneratedRegex("""(url\(['"]?.*?)%3F.*?(['"]?\))""")]
    private static partial Regex TrimmedQueryStringsFromUrlsRegex();
}