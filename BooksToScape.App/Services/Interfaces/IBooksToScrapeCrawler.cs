namespace BooksToScape.App.Services.Interfaces;

public interface IBooksToScrapeCrawler
{
    Task CrawlAsync(string url, string rootDownloadDirectory);
}
