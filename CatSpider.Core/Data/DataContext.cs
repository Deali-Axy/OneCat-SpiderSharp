using System;
using System.Collections.Generic;
using System.Text;
using CatSpider.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatSpider.Core.Data
{
    public class DataContext : DbContext
    {
        public DbSet<ListArticle> ListArticles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./spider.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 数据表的字段验证
            modelBuilder.Entity<ListArticle>(entity =>
            {
                entity.HasKey(e => e.Source);
                entity.Property(e => e.Title).IsRequired();
            });
        }
    }
}
