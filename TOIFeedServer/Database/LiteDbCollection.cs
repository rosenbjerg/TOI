using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace TOIFeedServer.Database
{
    public class LiteDbCollection<T> : IDbCollection<T>
    {
        private readonly LiteCollection<T> _collection;

        public LiteDbCollection(LiteCollection<T> collection)
        {
            _collection = collection;
        }

        public Task<DatabaseStatusCode> Insert(params T[] items)
        {
            return Task.FromResult(_collection.InsertBulk(items) == items.Length
                ? DatabaseStatusCode.Created
                : DatabaseStatusCode.AlreadyContainsElement);
        }

        public Task<DatabaseStatusCode> Update(string id, T item)
        {
            return Task.FromResult(_collection.Update(id, item)
                ? DatabaseStatusCode.Updated
                : DatabaseStatusCode.NoElement);
        }

        public Task<DatabaseStatusCode> Delete(string id)
        {
            return Task.FromResult(_collection.Delete(id)
                ? DatabaseStatusCode.Deleted
                : DatabaseStatusCode.NoElement);
        }

        public Task<DbResult<IEnumerable<T>>> Find(Expression<Func<T, bool>> predicate)
        {
            var result = _collection.Find(predicate).ToList();
            return Task.FromResult(new DbResult<IEnumerable<T>>(result, result.Any()
                ? DatabaseStatusCode.Ok
                : DatabaseStatusCode.NoElement));
        }

        public Task<DbResult<T>> FindOne(Expression<Func<T, bool>> predicate)
        {
            var result = _collection.FindOne(predicate);
            return Task.FromResult(new DbResult<T>(result, result != null
                ? DatabaseStatusCode.Ok
                : DatabaseStatusCode.NoElement));
        }

        public Task<DbResult<T>> FindOne(string id)
        {
            var result = _collection.FindById(id);
            return Task.FromResult(new DbResult<T>(result, result != null
                ? DatabaseStatusCode.Ok
                : DatabaseStatusCode.NoElement));
        }

        public Task<DbResult<IEnumerable<T>>> GetAll()
        {
            return Task.FromResult(new DbResult<IEnumerable<T>>(_collection.FindAll(), DatabaseStatusCode.Ok));
        }
    }
}