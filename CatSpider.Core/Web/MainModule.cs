using System;
using System.Collections.Generic;
using System.Text;
using Nancy;
using Newtonsoft.Json;

namespace CatSpider.Core.Web
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get("/", _ => "hello");
            Get("404", _ => HttpStatusCode.NotFound);

            Get("test", _ =>
            {
                var response = (Response)JsonConvert.SerializeObject(new int[] { 1, 2, 3 });
                response.ContentType = "application/json";
                return response;
            });


            Get("test2", _ => JsonConvert.SerializeObject(new int[] { 1, 2, 3 }));
        }
    }
}
