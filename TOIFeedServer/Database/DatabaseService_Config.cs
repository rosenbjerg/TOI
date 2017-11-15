using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        private MongoClient _client;
        private IMongoDatabase _database;
        private IDbCollection<ToiModel> _tois;
        private IDbCollection<TagModel> _tags;
        private IDbCollection<ContextModel> _ctxs;

        public DatabaseService(bool test = false)
        {
            if (!test)
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
                _client = new MongoClient(clientSettings);
                _database = _client.GetDatabase("TOI");
                _tois = new MongoDbCollection<ToiModel>(_database.GetCollection<ToiModel>("tois"));
                _tags = new MongoDbCollection<TagModel>(_database.GetCollection<TagModel>("tags"));
                _ctxs = new MongoDbCollection<ContextModel>(_database.GetCollection<ContextModel>("contexts"));
            }
            else
            {
                _tois = new InMemoryDbCollection<ToiModel>();
                _tags = new InMemoryDbCollection<TagModel>();
                _ctxs = new InMemoryDbCollection<ContextModel>();
            }
        }

        public async Task<DatabaseStatusCode> TruncateDatabase()
        {
            await _tois.DeleteAll();
            await _tags.DeleteAll();
            await _ctxs.DeleteAll();
            return DatabaseStatusCode.Ok;
        }
    }
}
