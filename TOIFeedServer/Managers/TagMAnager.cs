using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RedHttpServerCore.Request;
using RedHttpServerCore.Response;
using TOIClasses;
using TOIFeedServer.Models;

namespace TOIFeedServer.Managers
{
    class TagManager
    {
        public async Task AllTags(RRequest req, RResponse res)
        {
            try
            {
                var str = await req.ParseBodyAsync<string>();
                Console.WriteLine("body: " + str);
                var guids = JsonConvert.DeserializeObject<HashSet<Guid>>(str);
                if (guids == null)
                {
                    Console.WriteLine("Invalid body, no tags included");
                    return;
                }
                var tagInfo = (await res.ServerPlugins.Use<DatabaseService>().GetToisByTagIds(guids)).Result
                    .Select(x => x.TagInfoModel).ToList();
                Console.WriteLine(
                    $"Received request. Sending {tagInfo.Count} tags.");
                await res.SendJson(tagInfo);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.StackTrace);
            }
        }

        public static Guid CreateTagGuid(string bdAddr)
        {
            return Guid.ParseExact(bdAddr.Replace(":", string.Empty).PadLeft(32, '0'), "N");
        }
    }
}
