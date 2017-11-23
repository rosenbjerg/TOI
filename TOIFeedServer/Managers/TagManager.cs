using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TOIClasses;
using TOIFeedServer.Database;

namespace TOIFeedServer.Managers
{
    public class TagManager
    {
        private readonly DatabaseService _dbService;

        public TagManager(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<DbResult<IEnumerable<TagModel>>> GetTags(HashSet<string> ids = null)
        {
            if (ids == null)
                return await _dbService.GetAllTags();
            return await _dbService.GetTagsFromIds(ids);
        }

        

        private static TagModel ValidateTagForm(IFormCollection form, bool update)
        {
            var fields = new List<string> { "title", "longitude", "latitude", "radius", "type", "id" };
            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]))) 
                return null;

            if (!int.TryParse(form["radius"][0], out var radius) || 
                radius < 1 ||
                !decimal.TryParse(form["longitude"][0].Replace(",", "."), out var longitude) ||
                !decimal.TryParse(form["latitude"][0].Replace(",", "."), out var latitude))
                return null;
            
            return new TagModel
            {
                Title = form["title"][0],
                Id = form["id"][0],
                Radius = radius,
                Longitude = longitude,
                Latitude = latitude,
                Type = ParseTagType(form["type"][0])
            };
        }

        private static TagType ParseTagType(string type)
        {
            switch (type)
            {
                case "wifi":
                    return TagType.Wifi;
                case "ble":
                    return TagType.Bluetooth;
                case "gps":
                    return TagType.Gps;
                case "nfc":
                    return TagType.Nfc;
                default:
                    return TagType.Gps;
            }
        }

        public async Task<TagModel> CreateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form, false);
            if (tag == null) return null;
            return await _dbService.InsertTag(tag) != DatabaseStatusCode.Created ? null : tag;
        }

        public async Task<TagModel> UpdateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form, true);
            if (tag == null) return null;
            return await _dbService.UpdateTag(tag) != DatabaseStatusCode.Updated ? null : tag;
        }

        public async Task<TagModel> GetTag(IQueryCollection queries)
        {
            try
            {
                //trim query så den er ROBUST!    
                var id = queries["id"][0];
                var dbRes = await _dbService.GetTagFromId(id);
                return dbRes.Result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                return null;
            }
        }
    }
}