using System;
using LiteDB;
using MongoDB.Driver;
using TOIClasses;
using TOIFeedServer.Managers;

namespace TOIFeedServer
{
    public static class DatabaseFactory
    {
        public static Database BuildDatabase(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MongoDB:
                    return BuildMongoDatabase();
                case DatabaseType.LiteDB:
                    return BuildLiteDatabase();
                case DatabaseType.InMemory:
                    return BuildInMemoryDatabase();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static Database BuildMongoDatabase()
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress("localhost", 27017),
                ClusterConfigurator = builder =>
                {
                    builder.ConfigureCluster(settings =>
                        settings.With(serverSelectionTimeout: TimeSpan.FromSeconds(5)));
                }
            };
            var client = new MongoClient(clientSettings);
            var database = client.GetDatabase("TOI");
            return new Database(
                new MongoDbCollection<TagModel>(database.GetCollection<TagModel>("tags")),
                new MongoDbCollection<ToiModel>(database.GetCollection<ToiModel>("tois")),
                new MongoDbCollection<ContextModel>(database.GetCollection<ContextModel>("contexts")),
                new MongoDbCollection<User>(database.GetCollection<User>("users")));
        }

        private static Database BuildInMemoryDatabase()
        {
            return new Database(
                new InMemoryDbCollection<TagModel>(),
                new InMemoryDbCollection<ToiModel>(),
                new InMemoryDbCollection<ContextModel>(),
                new InMemoryDbCollection<User>());
        }

        private static Database BuildLiteDatabase()
        {
            var ldb = new LiteDatabase("toi.litedb");
            return new Database(
                new LiteDbCollection<TagModel>(ldb.GetCollection<TagModel>("tags")),
                new LiteDbCollection<ToiModel>(ldb.GetCollection<ToiModel>("tois")),
                new LiteDbCollection<ContextModel>(ldb.GetCollection<ContextModel>("contexts")),
                new LiteDbCollection<User>(ldb.GetCollection<User>("users")));
        }

        public enum DatabaseType
        {
            MongoDB,
            LiteDB,
            InMemory
        }
    }
}
