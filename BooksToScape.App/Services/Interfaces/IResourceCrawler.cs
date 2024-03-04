using FluentResults;

namespace BooksToScape.App.Services.Interfaces;

public interface IResourceCrawler
{
    Task<Result> DownloadLocalResourcesAsync(List<Uri> resourceUris, string rootDownloadDirectory);
}
