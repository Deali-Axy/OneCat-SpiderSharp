<Query Kind="Statements">
  <Output>DataGrids</Output>
  <NuGetReference>AngleSharp</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>AngleSharp.Html.Parser</Namespace>
  <Namespace>AngleSharp.Html.Dom</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

var http = new HttpClient();
var parser = new HtmlParser();

Enumerable.Range(1, 10)  // for循环转换为LINQ
    .AsParallel()         // 将LINQ并行化
    .AsOrdered()          // 按顺序保存结果（注意并非按顺序执行）
    .SelectMany(page =>
    {
        return Task.Run(async() => // 非异步代码使用async/await，需要包一层Task
        {
            string pageData = await http.GetStringAsync($"https://www.cnblogs.com/sitehome/p/{page}".Dump());
            IHtmlDocument doc = await parser.ParseDocumentAsync(pageData);
            return doc.QuerySelectorAll(".post_item").Select(tag => new
            {
                Title = tag.QuerySelector(".titlelnk").TextContent,
                Page = page,
                UserName = tag.QuerySelector(".post_item_foot .lightblue").TextContent,
                PublishTime = DateTime.Parse(Regex.Match(tag.QuerySelector(".post_item_foot").ChildNodes[2].TextContent, @"(\d{4}\-\d{2}\-\d{2}\s\d{2}:\d{2})", RegexOptions.None).Value),
                CommentCount = int.Parse(tag.QuerySelector(".post_item_foot .article_comment").TextContent.Trim()[3..^1]),
                ViewCount = int.Parse(tag.QuerySelector(".post_item_foot .article_view").TextContent[3..^1]),
                BriefContent = tag.QuerySelector(".post_item_summary").TextContent.Trim(),
            });
        }).GetAwaiter().GetResult(); // 等待Task执行完毕
    }).Dump();