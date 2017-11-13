using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        public async Task<DatabaseStatusCode> InsertTag(params TagModel[] tags)
        {
            await _tags.InsertManyAsync(tags);
            return DatabaseStatusCode.Created;
        }

        public async Task<DatabaseStatusCode> UpdateTag(TagModel tagModel)
        {
            await _tags.FindOneAndReplaceAsync(tagModel.TagId, tagModel);
            return DatabaseStatusCode.Updated;
        }

        public async Task<DbResult<TagModel>> GetTagFromId(string id)
        {
            var tag = await _tags.FindAsync(id);
            var status = !tag.Current.Any() ? DatabaseStatusCode.NoElement : DatabaseStatusCode.Ok;
            return new DbResult<TagModel>(tag.Current.FirstOrDefault(), status);
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTagsFromIds(HashSet<string> ids)
        {
            var tags = await _tags.FindAsync(t => ids.Contains(t.TagId));
            var statsCode = await tags.AnyAsync() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<TagModel>>(tags.Current, statsCode);
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetAllTags()
        {
            var tags = await _tags.FindAsync(t => true);
            var statsCode = tags.Any() ? DatabaseStatusCode.Ok : DatabaseStatusCode.NoElement;
            return new DbResult<IEnumerable<TagModel>>(tags.Current, statsCode);
        }
    }
}
