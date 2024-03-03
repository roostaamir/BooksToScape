using BooksToScape.App.Services.Interfaces;

namespace BooksToScape.App.Services;

public class GreedyBooksToScrapeCrawler : IBooksToScrapeCrawler
{
    public Task CrawlAsync(string url, string rootDownloadDirectory)
    {
        throw new NotImplementedException();
    }
}
