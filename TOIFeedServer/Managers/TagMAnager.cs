using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Console.WriteLine(req.ServerPlugins.Use<DatabaseService>().GetAllToiModels().Count());
                var guids = await req.ParseBodyAsync<HashSet<Guid>>();
                var tagInfo = res.ServerPlugins.Use<DatabaseService>().GetToisByTagIds(guids)
                    .Select(x => x.TagInfoModel.GetTagInfo()).ToList();
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
