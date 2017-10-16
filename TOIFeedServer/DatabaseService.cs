using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TOIFeedServer
{
    public class DatabaseService
    {
        private ToiModelContext db;
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
    }
    
}
