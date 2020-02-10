<Query Kind="Statements">
  <NuGetReference>AngleSharp</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
</Query>

var client = new HttpClient();
var parser = new HtmlParser();
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
var response = await client.GetAsync("http://news.cnr.cn/");
var bytes = await response.Content.ReadAsByteArrayAsync();
var resStr = Encoding.GetEncoding("gb2312").GetString(bytes);
var dom = new HtmlParser().ParseDocument(resStr);

dom.QuerySelectorAll(".contentPanel .lh30 a").Select((elem, result) =>
{
    return new
    {
        Title = elem.TextContent,
        Link = elem.GetAttribute("href")
    };
}).Dump();
