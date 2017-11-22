using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOIClasses;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        /// Toi database operations
        public async Task<DatabaseStatusCode> InsertToiModel(params ToiModel[] toiModel)
        {
            return await _db.Tois.Insert(toiModel);
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByTagIds(IEnumerable<string> ids)
        {
            var hash = ids.ToHashSet();
            return await _db.Tois.Find(p => p.Tags.Any(x => hash.Contains(x)));
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetAllToiModels()
        {
            return await _db.Tois.GetAll();
        }

        public async Task<DatabaseStatusCode> UpdateToiModel(ToiModel toi)
        {
            await _db.Tois.Update(toi.Id, toi);
            return DatabaseStatusCode.Ok;
        }

        public async Task<DbResult<ToiModel>> GetToi(string guid)
        {
            return await _db.Tois.FindOne(guid);
        }

        // TODO Create test for this method 
        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByContext(HashSet<string> contexts)
        {
            return await _db.Tois.Find(t => t.Contexts.Any(contexts.Contains));
        }
    }
}
