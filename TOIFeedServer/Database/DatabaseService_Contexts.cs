using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{

    public partial class DatabaseService
    {
        public async Task<DatabaseStatusCode> InsertContext(params ContextModel[] contexts)
        {
            await _ctxs.InsertManyAsync(contexts);
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<ContextModel>> GetContextFromId(string id)
        {
            var context = await _ctxs.FindAsync(id);
            var status = context == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<ContextModel>(await context.FirstOrDefaultAsync(), status);
        }

        public async Task<DatabaseStatusCode> UpdateContext(ContextModel context)
        {
            await _ctxs.FindOneAndReplaceAsync(c => c.Id == context.Id, context);
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetAllContexts()
        {
            var list = await _ctxs.Find(c => true).ToListAsync();
            var statusCode = list.Count == 0 ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<IEnumerable<ContextModel>>(list, statusCode);
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetContextsFromId(HashSet<string> contextIds)
        {
            var context = await _ctxs.FindAsync(c => contextIds.Contains(c.Id));
            var status = await context.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ContextModel>>(context.Current, status);
        }
    }
}
