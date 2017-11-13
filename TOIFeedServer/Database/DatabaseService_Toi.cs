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
        /// Toi database operations
        public async Task<DatabaseStatusCode> InsertToiModel(ToiModel toiModel)
        {
            if (await _db.Tois.AnyAsync(t => t.Equals(toiModel)))
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            await _db.Tois.AddAsync(toiModel);
            _db.SaveChanges();
            return DatabaseStatusCode.Created;
        }

        public async Task<DatabaseStatusCode> InsertToiModelList(List<ToiModel> toiModelList)
        {
            if (await _db.Tois.AnyAsync(t => toiModelList.Contains(t)))
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            if (toiModelList.Count != toiModelList.Distinct().Count())
            {
                return DatabaseStatusCode.ListContainsDuplicate;
            }
            await _db.Tois.AddRangeAsync(toiModelList);
            _db.SaveChanges();
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByTagIds(IEnumerable<Guid> ids)
        {
            var hash = ids.ToHashSet();
            var result = _db.Tois.Where(p => p.TagModels.Any(x => hash.Contains(x.TagId)));
            var statusCode = await result.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result, statusCode);
        }

        public async Task<DbResult<IEnumerable<ToiModel>>> GetAllToiModels()
        {
            var result = await _db.Tois
                .Include(t => t.ContextModels)
                .Include(t => t.TagModels)
                .ToListAsync();
            var statusCode = result.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result, statusCode);

        }

        public async Task<DatabaseStatusCode> UpdateToiModel(ToiModel toi)
        {
            var oldTagDbRes = await GetToi(toi.Id);
            var fetched = oldTagDbRes.Result;
            if (fetched == null)
            {
                return DatabaseStatusCode.NoElement;
            }
            
            
            fetched.Description = toi.Description;
            fetched.Image = toi.Image;
            fetched.Title = toi.Title;
            fetched.Url = toi.Url;

            fetched.ContextModels.RemoveAll(t => !toi.ContextModels.Contains(t));
            fetched.ContextModels.AddRange(toi.ContextModels.Intersect(fetched.ContextModels));
            
            fetched.TagModels.RemoveAll(t => !toi.TagModels.Contains(t));
            fetched.TagModels.AddRange(toi.TagModels.Where(t => !fetched.TagModels.Contains(t)));
            _db.Tois.Update(fetched);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Ok;
        }

        public async Task<DbResult<ToiModel>> GetToi(Guid guid)
        {
            var result = await _db.Tois
                .FirstOrDefaultAsync(toi => toi.Id == guid);
            var statusCode = result == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<ToiModel>(result, statusCode);
        }

        // TODO Create test for this method 
        public async Task<DbResult<IEnumerable<ToiModel>>> GetToisByContext(HashSet<Guid> contexts)
        {
            var result = _db.Tois
                .Include(t => t.ContextModels).ThenInclude(tcm => tcm.Context)
                .Include(t => t.TagModels).ThenInclude(ttm => ttm.Tag)
                .Where(t => contexts.Contains(t.Id));
            var statusCode = await result.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result, statusCode);
        }
    }
}
