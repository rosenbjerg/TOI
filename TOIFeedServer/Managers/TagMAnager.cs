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
            var guids = await req.ParseBodyAsync<HashSet<Guid>>();
            var tagInfo = res.ServerPlugins.Use<DatabaseService>().GetAllToiModels().Where(t => guids.Contains(t.Id)).ToList();
            Console.WriteLine($"Received request from {req.UnderlyingRequest.HttpContext.Connection.RemoteIpAddress}. Sending {tagInfo.Count} tags.");
            await res.SendJson(tagInfo);
        }

        public static Guid CreateTagGuid(string bdAddr)
        {
            return Guid.ParseExact(bdAddr.Replace(":", string.Empty).PadLeft(32, '0'), "N");
        }
    }
}
