using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LiteDB;
using TOIClasses;

namespace TOIFeedServer
{
    public class LiteDbCollection<T> : IDbCollection<T>
        where T : ModelBase
    {
        private readonly LiteCollection<T> _collection;
        private string[] _includes;

        public LiteDbCollection(LiteCollection<T> collection, params string[] includes)
        {
            _collection = collection;
            _includes = includes;
        }

        public Task<DatabaseStatusCode> Insert(params T[] items)
        {
            if (items.Length != items.Distinct().Count())
            {
                return Task.FromResult(DatabaseStatusCode.ListContainsDuplicate);
            }

            try
            {
                return Task.FromResult(_collection.InsertBulk(items) == items.Length
                    ? DatabaseStatusCode.Created
                    : DatabaseStatusCode.AlreadyContainsElement);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(DatabaseStatusCode.AlreadyContainsElement);
            }
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
            var result = _collection.Include("Tags").Include("Contexts").FindOne(predicate);
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
            var items = _collection.FindAll().ToList();
            var status = items.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;

            return Task.FromResult(new DbResult<IEnumerable<T>>(items, status));
        }

        public async Task<DatabaseStatusCode> DeleteAll()
        {
            var all = await GetAll();
            foreach (var item in all.Result)
            {
                _collection.Delete(item.Id);
            }
            return DatabaseStatusCode.Deleted;
        }
    }
}