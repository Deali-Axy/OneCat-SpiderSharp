using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CatSpider.Core.Models;
using Microsoft.Extensions.Configuration;

namespace CatSpider.Core.Data
{
    public class DataUtils
    {
        /// <summary>
        /// 用来初始化数据库，没有则新建数据库
        /// </summary>
        static DataUtils()
        {
            using (var context = new DataContext())
            {
                context.Database.EnsureCreated();
            }
        }
        /// <summary>
        /// 获取配置信息
        /// </summary>     
        private static IConfigurationRoot configuration;
        public static IConfigurationRoot Configuration
        {
            get
            {
                if (configuration == null)
                {
                    var builder = new ConfigurationBuilder();
                    configuration = builder.Build();
                }
                return configuration;
            }
        }
        public static void SaveData(ListArticle listArticle)
        {
            using (var context = new DataContext())
            {
                context.ListArticles.Add(listArticle);
                context.SaveChanges();
            }
        }
        public static void SaveOrUpdate(ListArticle listArticle)
        {
            using (var context = new DataContext())
            {
                ListArticle obj = context.ListArticles.Where(w => w.Link.Trim() == listArticle.Link.Trim()).FirstOrDefault();
                if (obj != null)
                {
                    obj.Title = listArticle.Title;
                    obj.Link = listArticle.Link;
                    obj.Source = listArticle.Source;
                    context.Update(obj);
                }
                else
                {
                    context.Add(listArticle);
                }
                context.SaveChanges();
            }
        }
        public static ListArticle ToListArticle(object model)
        {
            ListArticle listArticle = new ListArticle();
            PropertyInfo property = null;
            foreach (var item in typeof(ListArticle).GetProperties())
            {
                property = model.GetType().GetProperty(item.Name);
                if (property != null)
                {
                    item.SetValue(listArticle, property.GetValue(model));
                }
            }
            return listArticle;
        }
    }
}
