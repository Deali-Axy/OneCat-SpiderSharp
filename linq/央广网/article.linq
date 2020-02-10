<Query Kind="Statements">
  <NuGetReference>AngleSharp</NuGetReference>
  <NuGetReference>System.Net.Http</NuGetReference>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

var client = new HttpClient();
var parser = new HtmlParser();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
var response = await client.GetAsync("http://news.cnr.cn/native/gd/20200203/t20200203_524957949.shtml");
var bytes = await response.Content.ReadAsByteArrayAsync();
var resStr = Encoding.GetEncoding("gb2312").GetString(bytes);
var dom = new HtmlParser().ParseDocument(resStr);

var titleElem = dom.QuerySelector(".subject") ?? dom.QuerySelector(".article-header h1");

new{
    Title = titleElem.TextContent,
    Time = dom.QuerySelector(".source span").TextContent,
    TimeObj=DateTime.Parse(dom.QuerySelector(".source span").TextContent),
    Author = dom.QuerySelectorAll(".source span").Last().TextContent,
}.Dump();

dom.QuerySelectorAll(".TRS_Editor p").Select((elem, result) =>
{
    return elem.InnerHtml;
}).Dump();