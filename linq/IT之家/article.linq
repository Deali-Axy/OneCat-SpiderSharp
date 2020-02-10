<Query Kind="Statements">
  <NuGetReference>AngleSharp</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
</Query>

var client = new HttpClient();
string response = await client.GetStringAsync("https://www.ithome.com/0/470/669.htm");
var parser = new HtmlParser();
var dom = parser.ParseDocument(response);

var title = dom.QuerySelector(".post_title h1").TextContent;
var time_str = dom.QuerySelector("#pubtime_baidu").TextContent;
var author = dom.QuerySelector("#author_baidu strong").TextContent;
var content = dom.QuerySelector("#paragraph").InnerHtml;

new
{
    title = title,
    time = time_str,
    author = author,
    content = content
}.Dump();