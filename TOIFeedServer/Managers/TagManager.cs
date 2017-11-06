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

namespace TOIFeedServer.Managers
{
    public class TagManager
    {
        private readonly DatabaseService _dbService;

        public TagManager(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<List<TagModel>> GetTags(HashSet<Guid> ids = null)
        {
            var tois = await _dbService.GetAllToiModels();
//            var tags = await _dbService.GetAllToiModels()
            var tagInfos = await _dbService.GetToisByTagIds(ids);



//            var tags = tois.Result.



            return null;
        }

        public async Task AllTags(RRequest req, RResponse res)
        {
            try
            {
                var tags = (await req.ServerPlugins.Use<DatabaseService>().GetAllToiModels()).Result;
                foreach (var tag in tags)
                {
                    Console.WriteLine(tag.Id);
                }
                var guids = await req.ParseBodyAsync<HashSet<Guid>>();
                var tagInfo = (await res.ServerPlugins.Use<DatabaseService>().GetToisByTagIds(guids)).Result
                    .Select(x => x.GetToiInfo()).ToList();
                Console.WriteLine(
                    $"Received request. Sending {tagInfo.Count} tags.");
                await res.SendJson(tagInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        private static TagModel ValidateTagForm(IFormCollection form)
        {
            var fields = new List<string> { "title", "id", "longitude", "latitude", "radius", "type" };

            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]))) return null;
            if (!int.TryParse(form["radius"][0], out var radius) ||
                !double.TryParse(form["longitude"][0], out var longitude) ||
                !double.TryParse(form["latitude"], out var latitude) ||
                !int.TryParse(form["type"], out var type)) return null;
            if (radius < 10) return null;

            return new TagModel
            {
                Name = form["title"][0],
                TagId = CreateTagGuid(form["id"][0]),
                Radius = radius,
                Longtitude = longitude,
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

        public async Task GetTag(RRequest req, RResponse res)
        {
            try
            {
                //trim query så den er ROBUST!    
                var guid = Guid.ParseExact(req.Queries["id"][0].PadLeft(32, '0'), "N");
                var result = await res.ServerPlugins.Use<DatabaseService>().GetTagFromId(guid);
                await res.SendJson(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                throw;
            }
        }

        public static Guid CreateTagGuid(string bdAddr)
        {
            return Guid.ParseExact(bdAddr.Replace(":", string.Empty).PadLeft(32, '0'), "N");
        }
    }
}