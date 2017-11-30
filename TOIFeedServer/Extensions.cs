using System;
using System.Collections.Generic;
using TOIClasses;

namespace TOIFeedServer
{
    public static class Extensions
    {
        public static IEnumerable<string> SplitIds(string ids)
        {
            return ids.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static bool WithinRange(this TagModel tm, GpsLocation loc)
        {
            var a = tm.Latitude - loc.Latitude;
            var b = tm.Longitude - loc.Longtitude;
            var dist = Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            //Calculate the distance in meter, 111.325 km pr. degree
            var distInM = dist * 111.325 * 1000;

            return distInM <= tm.Radius;
        }
    }
}
