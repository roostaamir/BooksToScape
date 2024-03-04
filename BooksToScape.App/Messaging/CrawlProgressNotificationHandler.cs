using MediatR;

namespace BooksToScape.App.Messaging;

public class CrawlProgressNotificationHandler : INotificationHandler<CrawlProgressNotification>
{
    public Task Handle(CrawlProgressNotification notification, CancellationToken cancellationToken)
    {
        Console.Write($"\rProcessed {notification.ProcessedItemsCount} items");
        return Task.CompletedTask;
    }
}