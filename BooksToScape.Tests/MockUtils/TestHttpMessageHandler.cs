namespace BooksToScape.Tests.MockUtils;

public class TestHttpMessageHandler : HttpMessageHandler
{
    public virtual Task<HttpResponseMessage> SendRequestAsync(
        HttpRequestMessage message,
        string content)
    {
        throw new NotImplementedException("HttpMessageHandler SendAsync implementation missing");
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestContent =
            request.Content is null
                ? string.Empty
                : request.ContentToString();

        return SendRequestAsync(request, requestContent);
    }
}
