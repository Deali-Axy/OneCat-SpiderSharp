using System;
using System.Collections.Generic;
using System.Text;
using CatSpider.Core.Data;
using CatSpider.Core.Models;
using CatSpider.Core.Spider;
using Nancy;
using Newtonsoft.Json;

namespace CatSpider.Core.Web
{
    public class SpiderModule : NancyModule
    {
        public SpiderModule() : base("/spider")
        {
            Get("ithome/hot_list", async _ =>
            {
                var data = await ITHome.CrawlHotList().ConfigureAwait(false);
                var response = (Response) JsonConvert.SerializeObject(data);
                response.ContentType = "application/json";
                return response;
            });

            Get("ithome/new_list", async _ =>
            {
                var data = await ITHome.CrawlNewList().ConfigureAwait(false);
                var response = (Response) JsonConvert.SerializeObject(data);
                response.ContentType = "application/json";
                return response;
            });

            Get("ithome/crawl", _ =>
            {
                ITHome.Crawl();
                return "已经开始任务";
            });

            Get("ithome/article/{id}", async param =>
            {
                var id = param["id"];
                var context = SQLiteContextFactory.GetContext();
                ListArticle listArticle = context.QueryByKey<ListArticle>(id);
                var article = await ITHome.CrawlArticle(listArticle.Link);
                return JsonConvert.SerializeObject(article);
            });


            Get("cnblog/list/{page_count}", async param =>
            {
                var pageCount = param["page_count"];
                var data = await CnBlog.CrawlList(pageCount);
                return JsonConvert.SerializeObject(data);
            });
        }
    }
}