using System;
using System.Collections.Generic;
using System.Text;
using CatSpider.Core.Spider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatSpider.Test
{
    [TestClass]
    public class CnBlogTest
    {
        [TestMethod]
        public void TestCrawlList()
        {
            var data = CnBlog.CrawlList(2);
            Assert.IsTrue(data.Count > 0);
        }

        [TestMethod]
        public void TestCrawlList2()
        {
            var data = CnBlog.CrawlList2(2);
            while (!data.IsCompleted) { }
            Assert.IsTrue(data.Result.Count > 0);
        }

        [DataTestMethod]
        [DataRow("https://www.cnblogs.com/dudu/p/11562019.html")]
        [DataRow("https://www.cnblogs.com/JcrLive/p/12235715.html#4490478")]
        [DataRow("https://www.cnblogs.com/leipDao/p/10058144.html#4131620")]
        public void TestCrawlArticle(string url)
        {
            var data = CnBlog.CrawlArticle(url);
            while (!data.IsCompleted) { }
            Assert.IsNotNull(data.Result);
        }
    }
}
