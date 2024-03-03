using BooksToScape.App.Services;
using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace BooksToScape.Tests;

public class CrawlingCoreTests : PageTest
{
    private readonly string _testDirectory = Path.Combine(Environment.CurrentDirectory, "crawl-test");

    [SetUp]
    public void Setup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        Directory.CreateDirectory(_testDirectory);
    }

    [Test]
    public async Task TestCrawlingRootPage_ShouldNotHaveAnyConsoleErrors()
    {
        var crawler = new RootPageOnlyCrawler();
        await crawler.Crawl(_testDirectory);

        var consoleMessageList = new List<IConsoleMessage>();
        Page.Console += (_, msg) => consoleMessageList.Add(msg);
        await Page.GotoAsync($"File://{Path.Combine(_testDirectory, "index.html")}");

        await Expect(Page).ToHaveTitleAsync("All products | Books to Scrape - Sandbox");
        consoleMessageList.Should().BeEmpty();
    }

    [TearDown]
    public void Cleanup()
    {
        Directory.Delete(_testDirectory, true);
    }
}
