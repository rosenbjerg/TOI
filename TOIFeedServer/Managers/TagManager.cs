using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RedHttpServerCore.Request;
using RedHttpServerCore.Response;
using TOIClasses;
using TOIFeedServer.Database;
using TOIFeedServer.Models;
using static TOIFeedServer.Extensions;

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
            else
                return await _dbService.GetTagsFromIds(ids);
        }

        

        private static TagModel ValidateTagForm(IFormCollection form)
        {
            var fields = new List<string> { "title", "longitude", "latitude", "radius", "type" };

            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]))) return null;
            if (!int.TryParse(form["radius"][0], out var radius) ||
                !double.TryParse(form["longitude"][0], out var longitude) ||
                !double.TryParse(form["latitude"], out var latitude) ||
                !int.TryParse(form["type"], out var type)) return null;
            if (radius < 10) return null;

            return new TagModel
            {
                Name = form["title"][0],
                TagId = form["id"][0],
                Radius = radius,
                Longitude = longitude,
                Latitude = latitude,
                TagType = (TagType)type
            };
        }

        public async Task<bool> CreateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form);
            if (tag == null) return false;
            return await _dbService.InsertTag(tag) == DatabaseStatusCode.Created;
        }

        public async Task<bool> UpdateTag(IFormCollection form)
        {
            var tag = ValidateTagForm(form);
            if (tag == null) return false;
            return await _dbService.UpdateTag(tag) == DatabaseStatusCode.Updated;
        }

        public async Task<TagModel> GetTag(IQueryCollection queries)
        {
            try
            {
                //trim query så den er ROBUST!    
                var guid = queries["id"][0];
                var dbRes = await _dbService.GetTagFromId(guid);
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