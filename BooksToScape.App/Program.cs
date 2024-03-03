// See https://aka.ms/new-console-template for more information

using BooksToScape.App.Common;
using BooksToScape.App.Services;
using BooksToScape.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<IBooksToScrapeCrawler, RootPageOnlyCrawler>();
builder.Services.AddHttpClient(ApplicationConstants.DefaultHttpClientName);

using var host = builder.Build();
using var scope = host.Services.CreateScope();

Console.WriteLine("Enter a path to Crawl");
var path = Console.ReadLine();

var crawler = scope.ServiceProvider.GetRequiredService<IBooksToScrapeCrawler>();
await crawler.Crawl(path);

await host.StartAsync();
