﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedHttpServerCore.Request;
using RedHttpServerCore.Response;
using TOIClasses;
using TOIFeedServer.Database;
using TOIFeedServer.Models;

namespace TOIFeedServer.Managers
{
    class TagManager
    {
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

        public async Task CreateTag(RRequest req, RResponse res)
        {
            try
            {
                var tag = await req.ParseBodyAsync<TagModel>();
                var statusCode = await res.ServerPlugins.Use<DatabaseService>().InsertTag(tag);
                await res.SendJson(statusCode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
                throw;
            }
        }

        public async Task GetTag(RRequest req, RResponse res)
        {
            try
            { //trim query så den er ROBUST!    
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
