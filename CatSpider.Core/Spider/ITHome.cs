using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CatSpider.Core.Models;
using CatSpider.Core.Utils;
using CatSpider.Core.Data;
using NLog;
using NLog.Fluent;

namespace CatSpider.Core.Spider
{
    public class ITHome
    {
        /// <summary>
        /// 日志记录器
        /// 使用日志之前一定要检查配置文件是否有问题，可以开启 throwConfigExceptions 功能
        /// 参考：https://zhuanlan.zhihu.com/p/35469359
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static async Task<List<ListArticle>> CrawlHotList()
        {
            var dom = await HttpHelper.GetHtmlDocument("https://www.ithome.com/");

            var links = dom.QuerySelectorAll(".hot-list ul li a");
            var hotList = links.Select((elem, result) => new ListArticle
            {
                Title = elem.TextContent,
                Link = elem.GetAttribute("href"),
                Source = "IT之家"
            });

            return new List<ListArticle>(hotList);
        }


        public static async Task<List<ListArticle>> CrawlNewList()
        {
            var dom = await HttpHelper.GetHtmlDocument("https://www.ithome.com/");

            var links = dom.QuerySelectorAll(".new span a");
            var newList = links.Select((elem, result) => new ListArticle
            {
                Title = elem.TextContent,
                Link = elem.GetAttribute("href"),
                Source = "IT之家"
            });

            return new List<ListArticle>(newList);
        }

        public static async Task<Article> CrawlArticle(string url)
        {
            var dom = await HttpHelper.GetHtmlDocument(url);

            var title = dom.QuerySelector(".post_title h1").TextContent;
            var timeStr = dom.QuerySelector("#pubtime_baidu").TextContent;
            var author = dom.QuerySelector("#author_baidu strong").TextContent;
            var content = dom.QuerySelector("#paragraph").InnerHtml;

            return new Article
            {
                Title = title,
                Source = "IT之家",
                Link = url,
                Author = author,
                PublishTime = DateTime.Parse(timeStr),
                AddTime = DateTime.Now,
                Content = content
            };
        }

        public static async void Crawl()
        {
            var context = SQLiteContextFactory.GetContext();
            (await CrawlNewList().ConfigureAwait(true)).ForEach(obj =>
            {
                var query = context.Query<ListArticle>().Where(a => a.Source == obj.Source && a.Title == obj.Title);
                if (query.Count() > 0)
                {
                    logger.Debug($"内容已存在：{obj}");
                    return;
                }
                obj = context.Insert(obj);
                logger.Debug($"列表：{obj}");
                Console.WriteLine($"列表：{obj}");

            });

            (await CrawlHotList().ConfigureAwait(true)).ForEach(obj =>
            {
                var query = context.Query<ListArticle>().Where(a => a.Source == obj.Source && a.Title == obj.Title);
                if (query.Count() > 0)
                {
                    logger.Debug($"内容已存在：{obj}");
                    return;
                }
                obj = context.Insert(obj);
                logger.Debug($"列表：{obj}");
                Console.WriteLine($"列表：{obj}");
            });
        }
    }
}