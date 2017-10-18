using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public object GetTagFromID(int i)
        {
            return db.Tags.Where(t => t.TagId == i);
        }
    }
    
}
