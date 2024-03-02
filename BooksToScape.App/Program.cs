// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;

const string scrapingUrl = "https://books.toscrape.com/";

var client = new HttpClient();

var responseString = await client.GetStringAsync(scrapingUrl);

var htmlDocument = new HtmlDocument();
htmlDocument.LoadHtml(responseString);

var sideBarLinks = htmlDocument.DocumentNode
    .SelectNodes("//*[@id=\"default\"]/div/div/div/aside/div[2]/ul/li/ul/li[*]/a")
    .Select(x => x.Attributes["href"].Value)
    .ToList();

sideBarLinks.ForEach(Console.WriteLine);
