using System;
using System.Collections.Generic;
using System.Text;
using CatSpider.Core.Spider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatSpider.Test
{
    [TestClass]
    public class CNRadioNewsTest
    {
        [TestMethod]
        public void TestCrawlList()
        {
            var data = CNRadioNews.CrawlList();
            while (!data.IsCompleted) { }
            Assert.IsTrue(data.Result.Count > 0);
        }

        [DataTestMethod]
        [DataRow("http://news.cnr.cn/native/gd/20200203/t20200203_524957949.shtml")]
        [DataRow("http://news.cnr.cn/native/gd/20190927/t20190927_524795559.shtml")]
        [DataRow("http://news.cnr.cn/theory/gc/20190925/t20190925_524792023.shtml")]
        public void TestCrawlArticle(string url)
        {
            var data = CNRadioNews.CrawlArticle(url);
            while (!data.IsCompleted) { }
            Assert.IsNotNull(data.Result);
        }
    }
}
