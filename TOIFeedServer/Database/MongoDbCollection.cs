using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using TOIClasses;

namespace TOIFeedServer
{
    public class MongoDbCollection<T> : IDbCollection<T>
        where T : ModelBase
    {
        private IMongoCollection<T> _db;

        public MongoDbCollection(IMongoCollection<T> db)
        {
            _db = db;
        }

        public async Task<DatabaseStatusCode> Insert(params T[] items)
        {
            if (items.Length != items.Distinct().Count())
            {
                return DatabaseStatusCode.ListContainsDuplicate;
            }

            try
            {
                await _db.InsertManyAsync(items);
                return DatabaseStatusCode.Created;
            }
            catch (MongoBulkWriteException e)
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
        }

        public async Task<DatabaseStatusCode> Update(string id, T item)
        {
            var i = await _db.FindOneAndReplaceAsync(t => t.Id == id, item);
            return i != null ? DatabaseStatusCode.Updated : DatabaseStatusCode.NoElement;
        }

        public async Task<DatabaseStatusCode> Delete(string id)
        {
            var del = await _db.DeleteOneAsync(i => i.Id == id);
            if (del.DeletedCount > 0)
                return DatabaseStatusCode.Deleted;
            return DatabaseStatusCode.Error;
        }

        public async Task<DbResult<IEnumerable<T>>> Find(Expression<Func<T, bool>> predicate)
        {
            var result = await _db.Find(predicate).ToListAsync();
            var statusCode = result.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<T>>(result, statusCode);
        }

        public async Task<DbResult<T>> FindOne(Expression<Func<T, bool>> predicate)
        {
            var result = await _db.Find(predicate).FirstOrDefaultAsync();
            var statusCode = result != null ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<T>(result, statusCode);
        }

        public async Task<DbResult<T>> FindOne(string id)
        {
            var result = await _db.Find(i => i.Id == id).FirstOrDefaultAsync();
            var statusCode = result == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<T>(result, statusCode);
        }

        public async Task<DbResult<IEnumerable<T>>> GetAll()
        {
            var result = await _db.Find(FilterDefinition<T>.Empty).ToListAsync();
            var statusCode = result.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<T>>(result, statusCode);
        }

        public async Task<DatabaseStatusCode> DeleteAll()
        {
            await _db.DeleteManyAsync(FilterDefinition<T>.Empty);
            return DatabaseStatusCode.Deleted;
        }
    }
}
