﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using TOIFeedServer.Database;
using TOIFeedServer.Models;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer.Managers
{
    public class ToiManager
    {
        private readonly DatabaseService _dbService;

        public ToiManager(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        private async Task<ToiModel> ValidateToiForm(IFormCollection form)
        {
            var fields = new List<string> { "context", "tags", "title", "url", "description" };

            if (fields.Any(field => !form.ContainsKey(field) || string.IsNullOrEmpty(form[field][0]))) return null;
            if (!Guid.TryParseExact(form["context"].ToString().PadLeft(32, '0'), "N", out var contextId)) return null;
            HashSet<Guid> toiTags;
            try
            {
                toiTags = JsonConvert.DeserializeObject<List<string>>(form["tags"]).Select(GuidParse).ToHashSet();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

            var ctx = await _dbService.GetContextFromId(contextId);
            var tags = await _dbService.GetTagsFromId(toiTags);
            
            return new ToiModel
            {
                Id = Guid.NewGuid(),
                ContextModel = ctx.Result,
                TagModels = tags.Result.ToList(),
                Description = form["description"],
                Title = form["title"],
                Url = form["url"],
                Image = form.ContainsKey("image") ? form["image"] : StringValues.Empty
            };
        }

        public async Task<Guid> CreateToi(IFormCollection form)
        {
            var toi = await ValidateToiForm(form);
            if (toi == null) return Guid.Empty;
            else if (await _dbService.InsertToiModel(toi) == DatabaseStatusCode.Created)
                return toi.Id;
            else
                return Guid.Empty;
        }

        public async Task<bool> UpdateToi(IFormCollection form)
        {
            var toi = await ValidateToiForm(form);
            if (toi == null) return false;
            return await _dbService.UpdateToiModel(toi) == DatabaseStatusCode.Updated;
        }
        
//        public async Task AllTags(RRequest req, RResponse res)
//        {
//            try
//            {
//                var tags = (await req.ServerPlugins.Use<DatabaseService>().GetAllToiModels()).Result;
//                foreach (var tag in tags)
//                {
//                    Console.WriteLine(tag.Id);
//                }
//                var guids = await req.ParseBodyAsync<HashSet<Guid>>();
//                var tagInfo = (await res.ServerPlugins.Use<DatabaseService>().GetToisByTagIds(guids)).Result
//                    .Select(x => x.GetToiInfo()).ToList();
//                Console.WriteLine(
//                    $"Received request. Sending {tagInfo.Count} tags.");
//                await res.SendJson(tagInfo);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.Message + " " + e.StackTrace);
//            }
//        }
    }
}