// See https://aka.ms/new-console-template for more information

using BooksToScape.App.Services;
using HtmlAgilityPack;

const string scrapingUrl = "https://books.toscrape.com/";

var client = new HttpClient();

var responseString = await client.GetStringAsync(scrapingUrl);

var htmlDocument = new HtmlDocument();
htmlDocument.LoadHtml(responseString);

Console.WriteLine("Enter a path to Crawl");
var path = Console.ReadLine();

await new RootPageOnlyCrawler().Crawl(path);