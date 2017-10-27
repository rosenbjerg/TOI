using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TOIFeedServer.Models;

namespace TOIFeedServer
{
    public class DatabaseContext : DbContext
    {

        public DbSet<ToiModel> Tois { get; set; }
        public DbSet<TagModel> Tags { get; set; }
        public DbSet<PositionModel> Positions { get; set; }
        public DbSet<ContextModel> Contexts { get; set; }
        public DbSet<TagInfoModel> TagInfos { get; set; }

        public DatabaseContext()
        {

        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=toi.db");
            }
            else
            {
                
            }
        }
        
    }
}
