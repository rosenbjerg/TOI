using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Setup keys for the two relationship entities
            modelBuilder.Entity<ToiContextModel>().HasKey(tcm => new {tcm.ToiId, tcm.ContextId});
            modelBuilder.Entity<ToiTagModel>().HasKey(ttm => new {ttm.ToiId, ttm.TagId});
            
            //Configure many-to-many between TOI and Context
            modelBuilder.Entity<ToiModel>().HasMany(t => t.ContextModels).WithOne(tcm => tcm.Toi);
            modelBuilder.Entity<ContextModel>().HasMany(cm => cm.Tois).WithOne(tcm => tcm.Context);
            
            //Configure many-to-many between TOI and Tag
            modelBuilder.Entity<ToiModel>().HasMany(t => t.TagModels).WithOne(ttm => ttm.Toi);
            modelBuilder.Entity<TagModel>().HasMany(t => t.Tois).WithOne(ttm => ttm.Tag);
        }
    }
}
