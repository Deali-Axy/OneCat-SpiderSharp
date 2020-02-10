using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Threading.Tasks;

namespace CatSpider.Core.Utils
{
    public static class HttpHelper
    {
        private static readonly HttpClientHandler handler;
        private static readonly HttpClient client;

        public static HttpClientHandler Handler { get => handler; }
        public static HttpClient Client { get => client; }

        static HttpHelper()
        {
            handler = new HttpClientHandler();
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
        }

        public static async Task<IHtmlDocument> GetHtmlDocument(string url)
        {
            var html = await client.GetStringAsync(url);
            return new HtmlParser().ParseDocument(html);
        }

        public static async Task<IHtmlDocument> GetHtmlDocument(string url, string charset)
        {
            var res = await client.GetAsync(url);
            var resBytes = await res.Content.ReadAsByteArrayAsync();
            var resStr = Encoding.GetEncoding(charset).GetString(resBytes);
            return new HtmlParser().ParseDocument(resStr);
        }
    }
}
