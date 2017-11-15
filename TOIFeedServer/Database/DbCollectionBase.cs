using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public interface IDbCollection<T>
        where T : IModel
    {
        Task<DatabaseStatusCode> Insert(params T[] items);
        Task<DatabaseStatusCode> Update(string id, T item);
        Task<DatabaseStatusCode> Delete(string id);
        Task<DbResult<IEnumerable<T>>> Find(Expression<Func<T, bool>> predicate);
        Task<DbResult<T>> FindOne(Expression<Func<T, bool>> predicate);
        Task<DbResult<T>> FindOne(string id);
        Task<DbResult<IEnumerable<T>>> GetAll();

        Task<DatabaseStatusCode> DeleteAll();
        
    }
}
