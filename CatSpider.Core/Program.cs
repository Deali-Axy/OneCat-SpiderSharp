using AngleSharp.Html.Parser;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using LINQPad;
using CatSpider.Core.Data;
using CatSpider.Core.Models;
using CatSpider.Core.Spider;
using Nancy.Hosting.Self;
using NLog;

namespace CatSpider.Core
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string host = "http://localhost";
        private int port = 50010;
        private NancyHost nancy;

        public Program()
        {
            var uri = new Uri($"{host}:{port}/");
            nancy = new NancyHost(uri);
        }

        public void Start()
        {
            nancy.Start();
            logger.Debug($"nancy server started at {host}:{port}");
            Console.ReadKey();
            nancy.Stop();
        }

        static void Main(string[] args)
        {
            new Program().Start();
        }
    }
}
