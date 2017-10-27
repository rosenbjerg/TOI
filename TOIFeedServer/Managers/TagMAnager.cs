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
            var tagInfoList = new List<TagInfo>();
            res.ServerPlugins.Use<DatabaseService>().GetAllToiModels().ToList().ForEach(p => tagInfoList.Add(p.TagInfoModel.GetTagInfo()));
            await res.SendJson(tagInfoList);
            res.ServerPlugins.Use<DatabaseService>().TruncateDatabase();
        }

        public static Guid CreateTagGuid(string bdAddr)
        {
            return Guid.ParseExact(bdAddr.Replace(":", string.Empty).PadLeft(32, '0'), "N");
        }
    }
}
