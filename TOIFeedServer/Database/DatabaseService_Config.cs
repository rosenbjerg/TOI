using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        private MongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<ToiModel> _tois;
        private IMongoCollection<TagModel> _tags;
        private IMongoCollection<ContextModel> _ctxs;

        public DatabaseService(bool test = false)
        {
            _client = new MongoClient("mongodb://localhost:27017");
            _database = _client.GetDatabase("TOI");
            _tois = _database.GetCollection<ToiModel>("tois");
            _tags = _database.GetCollection<TagModel>("tags");
            _ctxs = _database.GetCollection<ContextModel>("contexts");
        }

        public async Task<DatabaseStatusCode> TruncateDatabase()
        {
            await _tois.DeleteManyAsync(t => true);
            await _tags.DeleteManyAsync(t => true);
            await _ctxs.DeleteManyAsync(t => true);
            return DatabaseStatusCode.Ok;
        }
    }
}
