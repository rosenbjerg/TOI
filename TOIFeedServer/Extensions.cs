using System;
using System.Collections.Generic;
using System.Text;

namespace TOIFeedServer
{
    public static class Extensions
    {
        public static Guid GuidParse(string strGuid)
        {
            return Guid.ParseExact(strGuid.PadLeft(32, '0'), "N");
        }
    }
}
