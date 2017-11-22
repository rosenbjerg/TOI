using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TOIFeedServer.Database;
using TOIFeedServer.Models;

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
            var always = new List<string> { "title", "longitude", "latitude", "radius" };
            var onUpdate = new List<string> {  "type" };

            if (always.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]))) return null;

            if (!update)
            {
                
            }
            if (!int.TryParse(form["radius"][0], out var radius) ||
                !double.TryParse(form["longitude"][0], out var longitude) ||
                !double.TryParse(form["latitude"], out var latitude) ||
                !int.TryParse(form["type"], out var type)) 
                return null;
            if (radius < 1) 
                return null;
            if (string.IsNullOrEmpty(form["id"][0]))
                return null;

            return new TagModel
            {
                Name = form["title"][0],
                Id = form["id"][0],
                Radius = radius,
                Longitude = longitude,
                Latitude = latitude,
                TagType = (TagType)type
            };
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