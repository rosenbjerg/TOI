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
        private DatabaseContext db;
        public DatabaseService(bool test = false)
        {
            var TDF = new ToiDbFactory();
            if (test)
            {
                db = TDF.CreateTestContext();
            }
            else
            {
                db = TDF.CreateContext();
            }
        }
        public void InsertToiModel(ToiModel toiModel)
        {
            db.Tois.Add(toiModel);
            db.SaveChanges();    
        }

        public async void InsertToiModelList(IEnumerable<ToiModel> toiModelList)
        {
            await db.Tois.AddRangeAsync(toiModelList);
            db.SaveChanges();
        }

        public IEnumerable<ToiModel> GetToiModelFromContext(string context)
        {
            return db.Tois.Where(s => s.Type == context);
        }

        public void InsertTag(TagModel tag)
        {
            db.Tags.Add(tag);
            db.SaveChanges();
        }

        public TagModel GetTagFromID(Guid i)
        {
            return db.Tags.SingleOrDefault(t => t.TagId == i);
        }

        public void InsertTags(IEnumerable<TagModel> tags)
        {
            db.Tags.AddRange(tags);
            db.SaveChanges();  
        }

        public void InsertContext(ContextModel context)
        {
            db.Contexts.Add(context);
            db.SaveChanges();
        }

        public ContextModel GetContextFromId(int id)
        {
            return db.Contexts.SingleOrDefault(c => c.Id == id);
        }

        public void InsertContexts(IEnumerable<ContextModel> contexts)
        {
            db.Contexts.AddRange(contexts);
            db.SaveChanges();
        }

        public IEnumerable<ContextModel> GetAllContexts()
        {
            return db.Contexts.Where(c => c.Id != -1);
        }

        public void InsertPosition(PositionModel position)
        {
            db.Positions.Add(position);
            db.SaveChanges();
        }

        public void TruncateDatabase()
        {
            var tags = db.Tags.Where(x => x.TagId != null);
            db.RemoveRange(tags);
            var tois = db.Tois.Where(x => x.Id != null);
            db.RemoveRange(tois);
            var positions = db.Positions.Where(x => x.Id != -1);
            db.RemoveRange(positions);
            var contexts = db.Contexts.Where(x => x.Id != -1);
            db.RemoveRange(contexts);
            db.SaveChanges();
        }

<<<<<<< Updated upstream
=======
        public void InsertToi(ToiModel model)
        {
            db.Tois.Add(model);
            db.SaveChanges();
        }

        public IEnumerable<ToiModel> GetToisByTagId(Guid tagId)
        {
            return db.Tois.Where(t => t.TagModel.TagId == tagId);
        }

>>>>>>> Stashed changes
        public IEnumerable<TagModel> GetTagsFromType(TagType type)
        {
            return db.Tags.Where(s => s.TagType == type);
        }

        public PositionModel GetPositionFromTagId(Guid tagId)
        {
            return db.Positions.FirstOrDefault(p => p.TagModelId == tagId);
        }

        public IEnumerable<ToiModel> GetToisByTagIds(IEnumerable<Guid> ids)
        {
            List<ToiModel> result = new List<ToiModel>();
            foreach (var id in ids)
            {
                result.Add(db.Tois.FirstOrDefault(p => p.TagModel.TagId == id));
            }
            return result;
        }
    }
    
}
