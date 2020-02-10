using System;
using System.Collections.Generic;
using System.Text;
using Chloe.SQLite;

namespace CatSpider.Core.Data
{
    public static class SQLiteContextFactory
    {
        /// <summary>
        /// 初始化数据库
        /// </summary>
        static SQLiteContextFactory()
        {
            
        }

        public static SQLiteContext GetContext()
        {
            string connString = "Data Source=CatSpider.db";
            return new SQLiteContext(new SQLiteConnectionFactory(connString));
        }
    }
}
