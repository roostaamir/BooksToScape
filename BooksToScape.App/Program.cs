﻿// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using BooksToScape.App.Common;
using BooksToScape.App.Services;
using BooksToScape.App.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddTransient<IBooksToScrapeCrawler, PerformantCrawler>();
builder.Services.AddTransient<IResourceCrawler, PerformantResourceCrawler>();
builder.Services.AddHttpClient(ApplicationConstants.DefaultHttpClientName);

builder.Services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Error);
});

using var host = builder.Build();
using var scope = host.Services.CreateScope();

Console.WriteLine("Enter a path to Crawl");
var path = Console.ReadLine();

var crawler = scope.ServiceProvider.GetRequiredService<IBooksToScrapeCrawler>();

var stopwatch = Stopwatch.StartNew();

await crawler.CrawlAsync(path);

stopwatch.Stop();
Console.WriteLine(stopwatch.ElapsedMilliseconds);

await host.StartAsync();
