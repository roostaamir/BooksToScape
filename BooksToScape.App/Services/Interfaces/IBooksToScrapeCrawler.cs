namespace BooksToScape.App.Services.Interfaces;

public interface IBooksToScrapeCrawler
{
    Task Crawl(string directoryToDownload);
}
