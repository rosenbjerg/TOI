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

        public DbResult<IEnumerable<TagModel>> GetTagsFromType(TagType type)
        {
            var tags = _db.Tags.Where(tag => tag.TagType == type);
            var statsCode = tags.Any() ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<IEnumerable<TagModel>>(tags, statsCode);
        }
    }
}
