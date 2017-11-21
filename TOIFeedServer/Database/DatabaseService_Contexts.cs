using System.Collections.Generic;
using System.Threading.Tasks;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{

    public partial class DatabaseService
    {
        public async Task<DatabaseStatusCode> InsertContext(params ContextModel[] contexts)
        {
            return await _db.Contexts.Insert(contexts);
        }

        public async Task<DbResult<ContextModel>> GetContextFromId(string id)
        {
            return await _db.Contexts.FindOne(id);
        }

        public async Task<DatabaseStatusCode> UpdateContext(ContextModel context)
        {
            return await _db.Contexts.Update(context.Id, context);
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetAllContexts()
        {
            return await _db.Contexts.GetAll();
        }

        public async Task<DbResult<IEnumerable<ContextModel>>> GetContextsFromIds(HashSet<string> contextIds)
        {
            return await _db.Contexts.Find(c => contextIds.Contains(c.Id));
        }

        public async Task<DatabaseStatusCode> DeleteContext(string id)
        {
            return await _db.Contexts.Delete(id);
        }
    }
}
