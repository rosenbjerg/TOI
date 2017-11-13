using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{

    public partial class DatabaseService
    {
        private DatabaseContext _db;
        public DatabaseService(bool test = false)
        {
            var tdf = new ToiDbFactory();
            _db = test ? tdf.CreateTestContext() : tdf.CreateContext();
        }


        public async Task<DatabaseStatusCode> InsertContext(ContextModel context)
        {
            if (await _db.Contexts.FindAsync(context.Id) != null)
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            await _db.Contexts.AddAsync(context);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<ContextModel>> GetContextFromId(Guid id)
        {
            var context = await _db.Contexts.SingleOrDefaultAsync(t => t.Id == id);
            var status = context == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<ContextModel>(context, status);
        }

        public async Task<DatabaseStatusCode> InsertContexts(List<ContextModel> contexts)
        {
            if (_db.Contexts.Any(t => contexts.Contains(t)))
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            if (contexts.Count() != contexts.Distinct().Count())
            {
                return DatabaseStatusCode.ListContainsDuplicate;
            }
            await _db.Contexts.AddRangeAsync(contexts);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetAllContexts()
        {
            var list = await _db.Contexts.ToListAsync();
            var statusCode = list.Count == 0 ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<IEnumerable<ContextModel>>(list, statusCode);
        }



        public async Task<DatabaseStatusCode> TruncateDatabase()
        {
            _db.RemoveRange(_db.Tags);
            _db.RemoveRange(_db.Tois);
            _db.RemoveRange(_db.Contexts);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Ok;
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetContextsFromId(HashSet<Guid> contextIds)
        {
            var context = _db.Contexts.Where(c => contextIds.Contains(c.Id));
            var status = await context.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ContextModel>>(context, status);
        }
    }
}
