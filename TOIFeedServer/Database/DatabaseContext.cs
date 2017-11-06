﻿using Microsoft.EntityFrameworkCore;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public class DatabaseContext : DbContext
    {

        public DbSet<ToiModel> Tois { get; set; }
        public DbSet<TagModel> Tags { get; set; }
        public DbSet<ContextModel> Contexts { get; set; }

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
        }
        
    }
}
