using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TOIFeedServer.Database
{
    public interface IDbCollection<T>
    {
        Task<DatabaseStatusCode> Insert(params T[] items);
        Task<DatabaseStatusCode> Update(string id, T item);
        Task<DatabaseStatusCode> Delete(string id);
        Task<DbResult<IEnumerable<T>>> Find(Expression<Func<T, bool>> predicate);
        Task<DbResult<T>> FindOne(Expression<Func<T, bool>> predicate);
        Task<DbResult<T>> FindOne(string id);
        Task<DbResult<IEnumerable<T>>> GetAll();
    }
}
