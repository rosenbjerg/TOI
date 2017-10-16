
using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;


namespace TOIFeedServer
{
    public class ToiDbFactory
    {
        private DbContextOptions<ToiModelContext> CreateOptions(SqliteConnection connection)
        {
            return new DbContextOptionsBuilder<ToiModelContext>()
                .UseSqlite(connection).Options;
        }
        public ToiModelContext CreateTestContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = CreateOptions(connection);
            using (var context = new ToiModelContext(options))
            {
                context.Database.Migrate();
            }

            return new ToiModelContext(options);
        }
        public ToiModelContext CreateContext()
        {
            var connection = new SqliteConnection("DataSource=toi.db");
            connection.Open();

            var options = CreateOptions(connection);

            return new ToiModelContext(options);
        }
      
    }
}
