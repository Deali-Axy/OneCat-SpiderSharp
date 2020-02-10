<Query Kind="Statements">
  <Output>DataGrids</Output>
  <NuGetReference>AngleSharp</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
</Query>

var client = new HttpClient();
string response = await client.GetStringAsync("https://www.ithome.com/");
var parser = new HtmlParser();
var dom = parser.ParseDocument(response);

var links = dom.QuerySelectorAll(".hot-list ul li a");
var hotList = links.Select((elem, result) => new
{
    title = elem.TextContent,
    link = elem.GetAttribute("href")
});


links = dom.QuerySelectorAll(".new span a");
var newList = links.Select((elem, result) => new
{
    title = elem.TextContent,
    link = elem.GetAttribute("href")
}).Dump();