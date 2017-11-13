using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        /// Toi database operations
        public async Task<DatabaseStatusCode> InsertToiModel(params ToiModel[] toiModel)
        {
            await _tois.InsertManyAsync(toiModel);
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByTagIds(IEnumerable<string> ids)
        {
            var hash = ids.ToHashSet();
            var result = await _tois.FindAsync(p => p.Tags.Any(x => hash.Contains(x)));
            var statusCode = result.Current.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result.Current, statusCode);
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetAllToiModels()
        {
            var result = await _tois.FindAsync(t => true);
            var statusCode = result.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result.Current, statusCode);

        }

        public async Task<DatabaseStatusCode> UpdateToiModel(ToiModel toi)
        {
            var oldTagDbRes = await GetToi(toi.Id);
            var fetched = oldTagDbRes.Result;
            if (fetched == null)
            {
                return DatabaseStatusCode.NoElement;
            }

            await _tois.FindOneAndReplaceAsync(toi.Id, toi);
            return DatabaseStatusCode.Ok;
        }

        public async Task<DbResult<ToiModel>> GetToi(string guid)
        {
            var result = await _tois.FindAsync(guid);
            var statusCode = !result.Current.Any() ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<ToiModel>(result.Current.FirstOrDefault(), statusCode);
        }

        // TODO Create test for this method 
        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByContext(HashSet<string> contexts)
        {
            var result = await _tois.FindAsync(t => contexts.Contains(t.Id));
            var statusCode = result.Current.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result.Current, statusCode);
        }
    }
}
