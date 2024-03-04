using System.Linq.Expressions;
using System.Net;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.Extensions;

namespace BooksToScape.Tests.MockUtils;

public class HttpMockHelper
{
    public TestHttpMessageHandler HttpMessageHandlerMock { get; } = Substitute.ForPartsOf<TestHttpMessageHandler>();

    public void SetUpHttpMock(
        string routePart,
        string response = "",
        HttpStatusCode responseCode = HttpStatusCode.OK,
        HttpMethod? method = null,
        IEnumerable<KeyValuePair<string, string?>>? headers = null)
    {
        Expression<Predicate<HttpRequestMessage>> expr = method == null
            ? requestMessage =>
                requestMessage.RequestUri!.AbsoluteUri.Equals(routePart, StringComparison.OrdinalIgnoreCase)
            : requestMessage =>
                requestMessage.Method == method &&
                requestMessage.RequestUri!.AbsoluteUri.Equals(routePart, StringComparison.OrdinalIgnoreCase);

        HttpMessageHandlerMock
            .Configure()
            .SendRequestAsync(Arg.Is(expr), Arg.Any<string>())
            .Returns(_ =>
            {
                var httpResponseMessage = new HttpResponseMessage(responseCode)
                {
                    Content = new StringContent(response)
                };

                foreach (var header in headers ?? Enumerable.Empty<KeyValuePair<string, string?>>())
                {
                    httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                return Task.FromResult(httpResponseMessage);
            });
    }

    public void SetUpHttpMock(
        string routePart,
        byte[]? response = null,
        HttpStatusCode responseCode = HttpStatusCode.OK,
        HttpMethod? method = null,
        IEnumerable<KeyValuePair<string, string?>>? headers = null)
    {
        Expression<Predicate<HttpRequestMessage>> expr = method == null
            ? requestMessage =>
                requestMessage.RequestUri!.AbsoluteUri.Equals(routePart, StringComparison.OrdinalIgnoreCase)
            : requestMessage =>
                requestMessage.Method == method &&
                requestMessage.RequestUri!.AbsoluteUri.Equals(routePart, StringComparison.OrdinalIgnoreCase);

        HttpMessageHandlerMock
            .Configure()
            .SendRequestAsync(Arg.Is(expr), Arg.Any<string>())
            .Returns(_ =>
            {
                var httpResponseMessage = new HttpResponseMessage(responseCode)
                {
                    Content = new ByteArrayContent(response ?? Array.Empty<byte>())
                };

                foreach (var header in headers ?? Enumerable.Empty<KeyValuePair<string, string?>>())
                {
                    httpResponseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                return Task.FromResult(httpResponseMessage);
            });
    }

    public void Reset()
    {
        HttpMessageHandlerMock.ClearSubstitute();
    }
}