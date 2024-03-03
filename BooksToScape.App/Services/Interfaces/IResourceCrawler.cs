namespace BooksToScape.App.Services.Interfaces;

public interface IResourceCrawler
{
    Task DownloadLocalResourcesAsync(List<Uri> resourceUris, string rootDownloadDirectory);
}
