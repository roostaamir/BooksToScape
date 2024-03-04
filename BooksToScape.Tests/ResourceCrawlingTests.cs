using System.Net;
using BooksToScape.App.Services;
using BooksToScape.App.Services.Interfaces;
using BooksToScape.Tests.MockUtils;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BooksToScape.Tests;

public class ResourceCrawlingTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpMockHelper _httpMockHelper = new HttpMockHelper();

    private readonly string _testDirectory = Path.Combine(Environment.CurrentDirectory, "crawl-test");

    public ResourceCrawlingTests()
    {
        _client = new HttpClient(_httpMockHelper.HttpMessageHandlerMock);
    }

    [SetUp]
    public void Setup()
    {
        _httpMockHelper.Reset();

        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        Directory.CreateDirectory(_testDirectory);
    }

    [Test]
    public async Task ResourceCrawlerDoesCrawling_ShouldSucceed([Values] bool usePerformantVersion)
    {
        var mockedCssResponse = await File.ReadAllTextAsync("Fixtures/Resources/test-resource-crawl.css");
        var mockedFontResponse = await File.ReadAllBytesAsync("Fixtures/Resources/glyphicons-halflings-regular.eot");

        _httpMockHelper.SetUpHttpMock(
            "https://books.toscrape.com/static/oscar/css/styles.css",
            mockedCssResponse);

        _httpMockHelper.SetUpHttpMock(
            "https://books.toscrape.com/static/oscar/fonts/glyphicons-halflings-regular.eot",
            mockedFontResponse);

        IResourceCrawler resourceCrawler = usePerformantVersion
            ? new PerformantResourceCrawler(_client, NullLogger<PerformantResourceCrawler>.Instance)
            : new ResourceCrawler(_client, NullLogger<ResourceCrawler>.Instance);

        var resourceUris = new List<Uri>
        {
            new Uri("https://books.toscrape.com/static/oscar/css/styles.css")
        };

        var result = await resourceCrawler.DownloadLocalResourcesAsync(resourceUris, _testDirectory);

        result.IsSuccess.Should().BeTrue();

        var scrapedCssFilePath = Path.Combine(_testDirectory, "static", "oscar", "css", "styles.css");
        File.Exists(Path.Combine(scrapedCssFilePath)).Should().BeTrue();
        (await File.ReadAllTextAsync(scrapedCssFilePath)).Should().BeEquivalentTo(mockedCssResponse);

        var scrapedFontFilePath = Path.Combine(_testDirectory, "static", "oscar", "fonts", "glyphicons-halflings-regular.eot");
        File.Exists(Path.Combine(scrapedFontFilePath)).Should().BeTrue();
    }

    [Test]
    public async Task ResourceCrawlerDoesCrawling_Gets404ForTheFont_ShouldHaveOneError([Values] bool usePerformantVersion)
    {
        var mockedCssResponse = await File.ReadAllTextAsync("Fixtures/Resources/test-resource-crawl.css");
        var mockedFontResponse = await File.ReadAllBytesAsync("Fixtures/Resources/glyphicons-halflings-regular.eot");

        _httpMockHelper.SetUpHttpMock(
            "https://books.toscrape.com/static/oscar/css/styles.css",
            mockedCssResponse);

        _httpMockHelper.SetUpHttpMock(
            "https://books.toscrape.com/static/oscar/fonts/glyphicons-halflings-regular.eot",
            (byte[]?) null,
            HttpStatusCode.NotFound);

        IResourceCrawler resourceCrawler = usePerformantVersion
            ? new PerformantResourceCrawler(_client, NullLogger<PerformantResourceCrawler>.Instance)
            : new ResourceCrawler(_client, NullLogger<ResourceCrawler>.Instance);

        var resourceUris = new List<Uri>
        {
            new Uri("https://books.toscrape.com/static/oscar/css/styles.css")
        };

        var result = await resourceCrawler.DownloadLocalResourcesAsync(resourceUris, _testDirectory);

        result.IsSuccess.Should().BeFalse();
        result.Errors
            .Select(error => error.Message).Should().ContainEquivalentOf(
                "Request to get https://books.toscrape.com/static/oscar/fonts/glyphicons-halflings-regular.eot failed with status code NotFound");

        var scrapedCssFilePath = Path.Combine(_testDirectory, "static", "oscar", "css", "styles.css");
        File.Exists(Path.Combine(scrapedCssFilePath)).Should().BeTrue();
        (await File.ReadAllTextAsync(scrapedCssFilePath)).Should().BeEquivalentTo(mockedCssResponse);

        var scrapedFontFilePath = Path.Combine(_testDirectory, "static", "oscar", "fonts", "glyphicons-halflings-regular.eot");
        File.Exists(Path.Combine(scrapedFontFilePath)).Should().BeFalse();
    }

    [TearDown]
    public void Cleanup()
    {
        Directory.Delete(_testDirectory, true);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}