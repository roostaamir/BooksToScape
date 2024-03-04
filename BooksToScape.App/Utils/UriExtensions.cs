namespace BooksToScape.App.Utils;

public static class UriExtensions
{
    public static string GetCleanedLocalDirectoryPath(this Uri uri)
    {
        var localPath = uri.LocalPath
            .TrimStart('/')
            .TrimEnd('/')
            .Replace('/', '\\');

        var queryParameterStartIndex = localPath.LastIndexOf('?');

        return queryParameterStartIndex < 0
            ? localPath
            : localPath.Substring(0, queryParameterStartIndex);
    }

    public static string GetLocalPathAndEnsureCreated(this Uri inputUri, string relativeToDirectory)
    {
        var localPath = Path.Combine(relativeToDirectory, inputUri.GetCleanedLocalDirectoryPath());
        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);

        return localPath;
    }
}
