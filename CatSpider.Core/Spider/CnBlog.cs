using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using CatSpider.Core.Data;
using CatSpider.Core.Models;
using CatSpider.Core.Utils;
using NLog;

namespace CatSpider.Core.Spider
{
    public class CnBlog
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 爬取文章列表
        /// </summary>
        /// <param name="page">设置爬到第几页</param>
        /// <returns></returns>
        public static List<ListArticle> CrawlList(int page = 10)
        {
            var http = new HttpClient();
            var parser = new HtmlParser();

            var data = Enumerable.Range(1, page) // for循环转换为LINQ
                .AsParallel() // 将LINQ并行化
                .AsOrdered() // 按顺序保存结果（注意并非按顺序执行）
                .SelectMany(page =>
                {
                    return Task.Run(async () => // 非异步代码使用async/await，需要包一层Task
                    {
                        string pageData = await http.GetStringAsync($"https://www.cnblogs.com/sitehome/p/{page}");
                        IHtmlDocument doc = await parser.ParseDocumentAsync(pageData);
                        return doc.QuerySelectorAll(".post_item").Select(tag => (ListArticle)new CnBlogListArticle
                        {
                            Title = tag.QuerySelector(".titlelnk").TextContent,
                            Source = "博客园",
                            Link = tag.QuerySelector(".titlelnk").GetAttribute("href"),
                            Page = page,
                            UserName = tag.QuerySelector(".post_item_foot .lightblue").TextContent,
                            PublishTime = DateTime.Parse(Regex.Match(tag.QuerySelector(".post_item_foot").ChildNodes[2].TextContent, @"(\d{4}\-\d{2}\-\d{2}\s\d{2}:\d{2})", RegexOptions.None).Value),
                            CommentCount = int.Parse(tag.QuerySelector(".post_item_foot .article_comment").TextContent.Trim()[3..^1]),
                            ViewCount = int.Parse(tag.QuerySelector(".post_item_foot .article_view").TextContent[3..^1]),
                            BriefContent = tag.QuerySelector(".post_item_summary").TextContent.Trim(),
                        });
                    }).GetAwaiter().GetResult(); // 等待Task执行完毕
                });
            return new List<ListArticle>(data);
        }

        /// <summary>
        /// 爬取文章列表的另一个实现
        /// </summary>
        /// <param name="page">页数</param>
        /// <returns></returns>
        public static async Task<List<CnBlogListArticle>> CrawlList2(int page = 10)
        {
            var http = HttpHelper.Client;
            var parser = new HtmlParser();

            var data = await Task.WhenAny(
                Enumerable.Range(1, page)
                .Select(async page =>
                {
                    string pageData = await http.GetStringAsync($"https://www.cnblogs.com/sitehome/p/{page}");
                    IHtmlDocument doc = await parser.ParseDocumentAsync(pageData);
                    return doc.QuerySelectorAll(".post_item").Select(tag => new CnBlogListArticle
                    {
                        Title = tag.QuerySelector(".titlelnk").TextContent,
                        Page = page,
                        UserName = tag.QuerySelector(".post_item_foot .lightblue").TextContent,
                        PublishTime = DateTime.Parse(Regex.Match(tag.QuerySelector(".post_item_foot").ChildNodes[2].TextContent, @"(\d{4}\-\d{2}\-\d{2}\s\d{2}:\d{2})", RegexOptions.None).Value),
                        CommentCount = int.Parse(tag.QuerySelector(".post_item_foot .article_comment").TextContent.Trim()[3..^1]),
                        ViewCount = int.Parse(tag.QuerySelector(".post_item_foot .article_view").TextContent[3..^1]),
                        BriefContent = tag.QuerySelector(".post_item_summary").TextContent.Trim(),
                    });
                })).ConfigureAwait(true);
            return new List<CnBlogListArticle>(await data);
        }

        /// <summary>
        /// 爬取文章
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<Article> CrawlArticle(string url)
        {
            var dom = await HttpHelper.GetHtmlDocument(url);

            var data = new Article
            {
                Title = dom.QuerySelector("#cb_post_title_url").TextContent,
                Source = "博客园",
                Content = dom.QuerySelector(".postBody").TextContent,
                Link = url,
                PublishTime = DateTime.Parse(dom.QuerySelector("#post-date").TextContent),
                AddTime = DateTime.Now,
                Author = dom.QuerySelector(".postDesc a").TextContent
            };

            Console.WriteLine(data.ToJson());

            return data;
        }

        public static void Crawl(int page=2)
        {
            var context = SQLiteContextFactory.GetContext();
            CrawlList(page).ForEach(obj =>
            {
                var query = context.Query<ListArticle>().Where(a => a.Source == obj.Source && a.Title == obj.Title);
                if (query.Count() > 0)
                {
                    logger.Debug($"内容已存在：{obj}");
                    return;
                }
                obj = context.Insert(obj);
                logger.Debug($"列表：{obj}");
            });
        }
    }
}