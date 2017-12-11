using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;
using MongoDB.Driver;
using TOIClasses;
using TOIFeedServer;
using TOIFeedServer.Managers;

namespace TOIFeedRepo.Database
{
    internal static class FeedRepoDatabaseFactory
    {
        public static FeedRepoDatabase Build(TOIFeedServer.DatabaseFactory.DatabaseType type)
        {
            switch (type)
            {
                case DatabaseFactory.DatabaseType.MongoDB:
                    return BuildMongoDatabase();
                case DatabaseFactory.DatabaseType.LiteDB:
                    return BuildLiteDatabase();
                case DatabaseFactory.DatabaseType.InMemory:
                    return BuildInMemoryDatabase();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static FeedRepoDatabase BuildMongoDatabase()
        {
            var clientSettings = new MongoClientSettings
            {
                Server = new MongoServerAddress("localhost", 27017),
                ClusterConfigurator = builder =>
                {
                    builder.ConfigureCluster(settings =>
                        settings.With(serverSelectionTimeout: TimeSpan.FromSeconds(5)));
                },
                //Credentials = new[] { MongoCredential.CreateCredential("TOI", "toi", "Tuborg Classic") }
            };
            var client = new MongoClient(clientSettings);
            var database = client.GetDatabase("FeedRepo");
            return new FeedRepoDatabase(
                new MongoDbCollection<Feed>(database.GetCollection<Feed>("feeds")),
                new MongoDbCollection<FeedOwner>(database.GetCollection<FeedOwner>("feedOwners")));
        }

        private static FeedRepoDatabase BuildInMemoryDatabase()
        {
            return new FeedRepoDatabase(
                new InMemoryDbCollection<Feed>(),
                new InMemoryDbCollection<FeedOwner>());
        }

        private static FeedRepoDatabase BuildLiteDatabase()
        {
            var ldb = new LiteDatabase("toi.litedb");
            return new FeedRepoDatabase(
                new LiteDbCollection<Feed>(ldb.GetCollection<Feed>("feeds")),
                new LiteDbCollection<FeedOwner>(ldb.GetCollection<FeedOwner>("feedOwners")));
        }
    }
}
