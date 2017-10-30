using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async void InsertToiModel(ToiModel toiModel)
        {
            await _db.Tois.AddAsync(toiModel);
            _db.SaveChanges();
        }

        public async void InsertToiModelList(IEnumerable<ToiModel> toiModelList)
        {
            await _db.Tois.AddRangeAsync(toiModelList);
            _db.SaveChanges();
        }

        public void InsertTag(TagModel tag)
        {
            _db.Tags.Add(tag);
            _db.SaveChanges();
        }

        public TagModel GetTagFromId(Guid i)
        {
            return _db.Tags.SingleOrDefault(t => t.TagId == i);
        }

        public void InsertTags(IEnumerable<TagModel> tags)
        {
            _db.Tags.AddRange(tags);
            _db.SaveChanges();
        }

        public void InsertContext(ContextModel context)
        {
            _db.Contexts.Add(context);
            _db.SaveChanges();
        }

        public ContextModel GetContextFromId(int id)
        {
            return _db.Contexts.SingleOrDefault(c => c.Id == id);
        }

        public void InsertContexts(IEnumerable<ContextModel> contexts)
        {
            _db.Contexts.AddRange(contexts);
            _db.SaveChanges();
        }

        public IEnumerable<ContextModel> GetAllContexts()
        {
            return _db.Contexts.Where(c => c.Id != -1);
        }

        public void InsertPosition(PositionModel position)
        {
            _db.Positions.Add(position);
            _db.SaveChanges();
        }

        public void TruncateDatabase()
        {
            var tags = _db.Tags.Where(x => x.TagId != null);
            _db.RemoveRange(tags);
            var tois = _db.Tois.Where(x => x.Id != null);
            _db.RemoveRange(tois);
            var positions = _db.Positions.Where(x => x.Id != -1);
            _db.RemoveRange(positions);
            var contexts = _db.Contexts.Where(x => x.Id != -1);
            _db.RemoveRange(contexts);
            _db.SaveChanges();
        }

        public IEnumerable<TagModel> GetTagsFromType(TagType type)
        {
            return _db.Tags.Where(s => s.TagType == type);
        }

        public PositionModel GetPositionFromTagId(Guid tagId)
        {
            return _db.Positions.FirstOrDefault(p => p.TagModelId == tagId);
        }

        public IEnumerable<ToiModel> GetToisByTagIds(IEnumerable<Guid> ids)
        {
            var hash = ids.ToHashSet();
            return _db.Tois.Where(p => p.TagModel.Any(x => hash.Contains(x.TagId)));
        }

        public IEnumerable<ToiModel> GetAllToiModels()
        {
            return _db.Tois.Where(p => p != null);
        }
    }

}
