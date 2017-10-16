using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TOIFeedServer
{

    public class ToiModelContext : DbContext
    {
        public DbSet<ToiModel> Tois { get; set; }
        public DbSet<TestFk> TestFK { get; set; }

        public ToiModelContext()
        {
            
        }

        public ToiModelContext(DbContextOptions<ToiModelContext> options)
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

    public class ToiModel
    {
        public ToiModel()
        {
            
        }
        public ToiModel(string type)
        {
            Type = type;
        }

        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }

    public class TestFk
    {
        [ForeignKey(nameof(Id))]
        public ToiModel ToiModel { get; set; }

        public int Id { get; set; }
    }

}
