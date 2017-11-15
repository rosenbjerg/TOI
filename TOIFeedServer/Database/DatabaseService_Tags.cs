using System.Collections.Generic;
using System.Threading.Tasks;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        public async Task<DatabaseStatusCode> InsertTag(params TagModel[] tags)
        {
            return await _db.Tags.Insert(tags);
        }

        public async Task<DatabaseStatusCode> UpdateTag(TagModel tagModel)
        {
            return await _db.Tags.Update(tagModel.Id, tagModel);
        }

        public async Task<DbResult<TagModel>> GetTagFromId(string id)
        {
            return await _db.Tags.FindOne(id);
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTagsFromIds(HashSet<string> ids)
        {
            return await _db.Tags.Find(t => ids.Contains(t.Id));
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetAllTags()
        {
            return await _db.Tags.GetAll();
        }
    }
}
