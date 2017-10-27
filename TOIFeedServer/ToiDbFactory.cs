
using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TOIFeedServer.Models;


namespace TOIFeedServer
{
    public class ToiDbFactory
    {
        private DbContextOptions<DatabaseContext> CreateOptions(SqliteConnection connection)
        {
            return new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(connection).Options;
        }
        public DatabaseContext CreateTestContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = CreateOptions(connection);
            using (var context = new DatabaseContext(options))
            {
                context.Database.Migrate();
            }

            return new DatabaseContext(options);
        }
        public DatabaseContext CreateContext()
        {
            var connection = new SqliteConnection("DataSource=toi.db");
            connection.Open();

            var options = CreateOptions(connection);
            using (var context = new DatabaseContext(options))
            {
                context.Database.Migrate();
            }

            return new DatabaseContext(options);
        }
      
    }
}
