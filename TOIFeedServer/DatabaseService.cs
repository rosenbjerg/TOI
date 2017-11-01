using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TOIFeedServer.Models;

namespace TOIFeedServer
{
    public class DatabaseService
    {
        private DatabaseContext _db;
        public DatabaseService(bool test = false)
        {
            var tdf = new ToiDbFactory();
            _db = test ? tdf.CreateTestContext() : tdf.CreateContext();
        }

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
            if(toiModelList.Count() != toiModelList.Distinct().Count())
            {
                return DatabaseStatusCode.ListContainsDuplicate;
            }
            await _db.Tois.AddRangeAsync(toiModelList);
            _db.SaveChanges();
            return DatabaseStatusCode.Created;
        }

        public async Task<DatabaseStatusCode> InsertTag(TagModel tag)
        {
            if (await _db.Tags.AnyAsync(t => t.Equals(tag)))
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            await _db.Tags.AddAsync(tag);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<TagModel>> GetTagFromId(Guid id)
        {
            var tag = await _db.Tags.FindAsync(id);
            var status = tag == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<TagModel>(tag, status);
        }


        public async Task<DatabaseStatusCode> InsertTags(List<TagModel> tags)
        {
            if (await _db.Tags.AnyAsync(t => tags.Contains(t)))
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            if (tags.Count() != tags.Distinct().Count())
            {
                return DatabaseStatusCode.ListContainsDuplicate;
            }
            await _db.Tags.AddRangeAsync(tags);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Created;

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


        public async Task<DatabaseStatusCode> InsertPosition(PositionModel position)
        {
            if (await _db.Positions.FindAsync(position.Id) != null)
            {
                return DatabaseStatusCode.AlreadyContainsElement;
            }
            await _db.Positions.AddAsync(position);
            await _db.SaveChangesAsync();

            return DatabaseStatusCode.Created;
        }

        public async Task<DatabaseStatusCode> TruncateDatabase()
        {
            _db.RemoveRange(_db.Tags);
            _db.RemoveRange(_db.Tois);
            _db.RemoveRange(_db.Positions);
            _db.RemoveRange(_db.Contexts);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Ok;
        }
        
        public DbResult<IEnumerable<TagModel>> GetTagsFromType(TagType type)
        {
            var tags = _db.Tags.Where(tag => tag.TagType == type);
            var statsCode = tags.Any() ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<IEnumerable<TagModel>>(tags, statsCode);
        }

        public async Task<DbResult<PositionModel>> GetPositionFromTagId(Guid tagId)
        {
            var result = await _db.Positions.FirstOrDefaultAsync(p => p.TagModelId == tagId);
            var statusCode = result == null ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<PositionModel>(result, statusCode);
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
    }
}
