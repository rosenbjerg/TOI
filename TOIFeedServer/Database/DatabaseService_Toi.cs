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
            if (toiModelList.Count() != toiModelList.Distinct().Count())
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
            var result = _db.Tois as IQueryable<ToiModel>;
            var statusCode = await result.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<ToiModel>>(result, statusCode);

        }

        public async Task<DatabaseStatusCode> UpdateToiModel(ToiModel toi)
        {
            var fetched = await _db.Tois.FindAsync(toi.Id);
            if (fetched == null)
            {
                return DatabaseStatusCode.NoElement;
            }
            if (toi.ContextModel == null && fetched.ContextModel != null)
                fetched.ContextModel = null;
            else if (toi.ContextModel != null && fetched.ContextModel == null)
                fetched.ContextModel = toi.ContextModel;
            else if (toi.ContextModel != null && !toi.ContextModel.Equals(fetched.ContextModel))
                fetched.ContextModel = (await GetContextFromId(toi.ContextModel.Id)).Result;

            fetched.Description = toi.Description;
            fetched.Image = toi.Image;
            fetched.Title = toi.Title;
            fetched.Url = toi.Url;

            fetched.TagModels.RemoveAll(t => !toi.TagModels.Contains(t));
            fetched.TagModels.AddRange(toi.TagModels.Intersect(fetched.TagModels));
            _db.Tois.Update(fetched);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Ok;
        }

        public async Task<DbResult<ToiModel>> GetToi(Guid guid)
        {
            var result = await _db.Tois.FindAsync(guid);
            var statusCode = result == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<ToiModel>(result, statusCode);
        }
    }
}
