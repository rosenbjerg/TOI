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

        public async Task<DatabaseStatusCode> UpdateTag(TagModel tagModel)
        {
            if (!await _db.Tags.AnyAsync(t => t.Equals(tagModel)))
            {
                return DatabaseStatusCode.NoElement;
            }

            var tag = (await GetTagFromId(tagModel.TagId)).Result;

            tag.Name = tagModel.Name;
            tag.Latitude = tagModel.Latitude;
            tag.Longitude = tagModel.Longitude;
            tag.Radius = tagModel.Radius;
            tag.TagType = tagModel.TagType;

            _db.Tags.Update(tag);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Updated;
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
            if (tags.Count != tags.Distinct().Count())
            {
                return DatabaseStatusCode.ListContainsDuplicate;
            }
            await _db.Tags.AddRangeAsync(tags);
            await _db.SaveChangesAsync();
            return DatabaseStatusCode.Created;
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTagsFromType(TagType type)
        {
            var tags = _db.Tags.Where(tag => tag.TagType == type);
            var statsCode = await tags.AnyAsync() ?  DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<TagModel>>(tags, statsCode);
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTagsFromId(HashSet<Guid> ids)
        {
            var tags = _db.Tags.Where(t => ids.Contains(t.TagId));
            var statsCode = await tags.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<TagModel>>(tags, statsCode);
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetAllTags()
        {
            var tags = await _db.Tags.ToListAsync();
            var statsCode = tags.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<TagModel>>(tags, statsCode);
        }
    }
}
