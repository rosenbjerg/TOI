using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        public async Task<DatabaseStatusCode> InsertTag(params TagModel[] tags)
        {
            return await _tags.Insert(tags);
        }

        public async Task<DatabaseStatusCode> UpdateTag(TagModel tagModel)
        {
            return await _tags.Update(tagModel.Id, tagModel);
        }

        public async Task<DbResult<TagModel>> GetTagFromId(string id)
        {
            return await _tags.FindOne(id);
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTagsFromIds(HashSet<string> ids)
        {
            return await _tags.Find(t => ids.Contains(t.Id));
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetAllTags()
        {
            return await _tags.GetAll();
        }
    }
}
