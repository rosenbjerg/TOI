using System;
using System.Collections.Generic;

namespace TOIFeedServer
{
    public static class Extensions
    {
        public static IEnumerable<string> SplitIds(string ids)
        {
            return ids.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
