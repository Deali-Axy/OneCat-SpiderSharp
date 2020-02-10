using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Chloe.Annotations;

namespace CatSpider.Core.Models
{
    [Table("Articles")]
    public class Article
    {
        [Column("Id", IsPrimaryKey = true)]
        [AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Link { get; set; }
        public string Author { get; set; }
        public DateTime PublishTime { get; set; }
        public DateTime AddTime { get; set; }
        public string Content { get; set; }

        public Dictionary<string, string> ToDict()
        {
            return new Dictionary<string, string>();
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                // 不编码中文字符
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            };
            return JsonSerializer.Serialize<Article>(this, options);
        }

        public string ToJson2()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }


    [Table("ListArticles")]
    public class ListArticle
    {
        [Column("Id", IsPrimaryKey = true)]
        [AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $"{Id}-{Source}-{Title}-{Link}";
        }
    }

    public class CnBlogListArticle : ListArticle
    {
        public int Page { get; set; }
        public string UserName { get; set; }
        public DateTime PublishTime { get; set; }
        public int CommentCount { get; set; }
        public int ViewCount { get; set; }
        public string BriefContent { get; set; }
    }
}
