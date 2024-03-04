using FluentResults;

namespace BooksToScape.App.Services.Interfaces;

public interface IBooksToScrapeCrawler
{
    Task<Result> CrawlAsync(string rootDownloadDirectory);
}
