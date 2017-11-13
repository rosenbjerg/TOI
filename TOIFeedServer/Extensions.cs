using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TOIFeedServer.Models;

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
