using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatSpider.Core.Data;
using CatSpider.Core.Models;
using CatSpider.Core.Utils;
using NLog;

namespace CatSpider.Core.Spider
{
    /// <summary>
    /// 央广网新闻
    /// </summary>
    public class CNRadioNews
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static CNRadioNews()
        {
            // 为了解码gb2312编码，需要nuget安装System.Text.Encoding.CodePages这个包
            // 并且注册CodePages
            // 参考：https://blog.csdn.net/koala_ivy/article/details/79869588
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static async Task<List<ListArticle>> CrawlList()
        {
            var dom = await HttpHelper.GetHtmlDocument("http://news.cnr.cn/", "gb2312");
            var data = dom.QuerySelectorAll(".contentPanel .lh30 a").Select((elem, result) =>
             {
                 return new ListArticle
                 {
                     Title = elem.TextContent,
                     Source = "央广网",
                     Link = elem.GetAttribute("href")
                 };
             });

            return new List<ListArticle>(data);
        }

        public static async Task<Article> CrawlArticle(string url)
        {
            var dom = await HttpHelper.GetHtmlDocument(url, "gb2312");

            var titleElem = dom.QuerySelector(".subject") ?? dom.QuerySelector(".article-header h1");

            var article = new Article
            {
                Title = titleElem.TextContent,
                PublishTime = DateTime.Parse(dom.QuerySelector(".source span").TextContent),
                AddTime = DateTime.Now,
                Author = dom.QuerySelectorAll(".source span").Last().TextContent,
                Content = "",
            };


            dom.QuerySelectorAll(".TRS_Editor p").Select((elem, result) =>
            {
                article.Content += elem.TextContent + Environment.NewLine;
                return elem.TextContent;
            });            

            return article;
        }

        public static async void Crawl()
        {
            var context = SQLiteContextFactory.GetContext();
            (await CrawlList().ConfigureAwait(true)).ForEach(obj =>
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
