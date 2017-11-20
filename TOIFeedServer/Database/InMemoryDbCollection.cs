using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public class InMemoryDbCollection<T> : IDbCollection<T>
        where T : IModel
    {
        public List<T> Store { get; set; } = new List<T>();
        
        public Task<DatabaseStatusCode> Insert(params T[] items)
        {
            if (items.Length != items.Distinct().Count())
            {
                return Task.FromResult(DatabaseStatusCode.ListContainsDuplicate);
            }
            if (items.Any(item => Store.Contains(item)))
            {
                return Task.FromResult(DatabaseStatusCode.AlreadyContainsElement);
            }

            Store.AddRange(items);
            return Task.FromResult(DatabaseStatusCode.Created);
        }

        public Task<DatabaseStatusCode> Update(string id, T item)
        {
            var i = Store.IndexOf(item);
            if (i == -1)
            {
                return Task.FromResult(DatabaseStatusCode.NoElement);
            }
            Store[i] = item;
            return Task.FromResult(DatabaseStatusCode.Updated);
        }

        public Task<DatabaseStatusCode> Delete(string id)
        {
            var itemIndex = Store.FindIndex(i => i.Id == id);
            if (itemIndex == -1)
            {
                return Task.FromResult(DatabaseStatusCode.NoElement);
            }
            Store.RemoveAt(itemIndex);
            return Task.FromResult(DatabaseStatusCode.Deleted);
        }

        public Task<DbResult<IEnumerable<T>>> Find(Expression<Func<T, bool>> predicate)
        {
            var items = Store.Where(predicate.Compile());
            var status = items.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;

            return Task.FromResult(new DbResult<IEnumerable<T>>(items, status));
        }

        public Task<DbResult<T>> FindOne(Expression<Func<T, bool>> predicate)
        {
            var item = Store.FirstOrDefault(predicate.Compile());
            var status = item != null ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;

            return Task.FromResult(new DbResult<T>(item, status));
        }

        public Task<DbResult<T>> FindOne(string id)
        {
            var item = Store.FirstOrDefault(i => i.Id == id);
            var status = item != null ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;

            return Task.FromResult(new DbResult<T>(item, status));
        }

        public Task<DbResult<IEnumerable<T>>> GetAll()
        {
            var status = Store.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return Task.FromResult(new DbResult<IEnumerable<T>>(Store, status));
        }

        public Task<DatabaseStatusCode> DeleteAll()
        {
            Store = new List<T>();
            return Task.FromResult(DatabaseStatusCode.Deleted);
        }
    }
}
