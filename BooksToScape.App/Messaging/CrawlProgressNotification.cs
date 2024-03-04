using MediatR;

namespace BooksToScape.App.Messaging;

public class CrawlProgressNotification(int processedItemsCount) : INotification
{
    public int ProcessedItemsCount { get; } = processedItemsCount;
}