using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using Chloe.Infrastructure;

namespace CatSpider.Core.Data
{
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        /// <summary>
        /// 数据库连接字符串，如下
        /// Data Source=dapperTest.db
        /// </summary>
        string _connString = null;
        public SQLiteConnectionFactory(string connString)
        {
            this._connString = connString;
        }
        public IDbConnection CreateConnection()
        {
            // 得先安装Sqlite的驱动
            // Microsoft.Data.Sqlite
            // System.Data.Sqlite
            SQLiteConnection conn = new SQLiteConnection(_connString);
            return conn;
        }
    }
}
