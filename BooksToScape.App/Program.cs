// See https://aka.ms/new-console-template for more information

using HtmlAgilityPack;
using StreamWriter = System.IO.StreamWriter;

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


//Testing html Manipulation
var headerElement = htmlDocument.DocumentNode
    .SelectSingleNode("//*[@id=\"default\"]/div/div/div/div/div[1]/h1");
headerElement.InnerHtml = "Other Title";

await using var writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "modified-doc.html"));
htmlDocument.Save(writer);
