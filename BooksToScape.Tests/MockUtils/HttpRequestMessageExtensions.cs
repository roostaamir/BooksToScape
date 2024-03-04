namespace BooksToScape.Tests.MockUtils;

public static class HttpRequestMessageExtensions
{
    public static string ContentToString(this HttpRequestMessage request)
    {
        var contentStream = request.Content!.ReadAsStream();
        contentStream.Seek(0, SeekOrigin.Begin);

        var reader = new StreamReader(contentStream);
        return reader.ReadToEnd();
    }
}
