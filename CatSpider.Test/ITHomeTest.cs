using System.Collections.Generic;
using CatSpider.Core.Models;
using CatSpider.Core.Spider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CatSpider.Test
{
    [TestClass]
    public class ITHomeTest
    {
        [TestMethod]
        public void TestCrawlHotList()
        {
            var data = ITHome.CrawlHotList();
            while (!data.IsCompleted) { }
            Assert.IsTrue(data.Result.Count > 0);
        }

        [TestMethod]
        public void TestCrawlNewList()
        {
            var data = ITHome.CrawlNewList();
            while (!data.IsCompleted) { }
            Assert.IsTrue(data.Result.Count > 0);
        }

        [DataTestMethod]
        [DataRow("https://www.ithome.com/0/471/151.htm")]
        [DataRow("https://www.ithome.com/0/471/086.htm")]
        [DataRow("https://www.ithome.com/0/471/075.htm")]
        public void TestCrawlArticle(string url)
        {
            var data = ITHome.CrawlArticle(url);
            while (!data.IsCompleted) { }
            Assert.IsNotNull(data.Result);
        }
    }
}
