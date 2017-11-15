using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{

    public partial class DatabaseService
    {
        public async Task<DatabaseStatusCode> InsertContext(params ContextModel[] contexts)
        {
            return await _ctxs.Insert(contexts);
        }

        public async Task<DbResult<ContextModel>> GetContextFromId(string id)
        {
            return await _ctxs.FindOne(id);
        }

        public async Task<DatabaseStatusCode> UpdateContext(ContextModel context)
        {
            return await _ctxs.Update(context.Id, context);
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetAllContexts()
        {
            return await _ctxs.GetAll();
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetContextsFromIds(HashSet<string> contextIds)
        {
            return await _ctxs.Find(c => contextIds.Contains(c.Id));
        }
    }
}
