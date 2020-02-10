<Query Kind="Statements">
  <NuGetReference>AngleSharp</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var http = new HttpClient();
var parser = new HtmlParser();

var html = await http.GetStringAsync("https://www.cnblogs.com/Peter-Luo/p/12254951.html");
var dom = parser.ParseDocument(html);

new
{
    Title = dom.QuerySelector("#cb_post_title_url").TextContent,
    Content = dom.QuerySelector(".postBody").TextContent,
    Time = dom.QuerySelector("#post-date").TextContent,
    Author=dom.QuerySelector(".postDesc a").TextContent
}.Dump();