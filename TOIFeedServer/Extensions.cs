using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TOIFeedServer
{
    public static class Extensions
    {
        public static IEnumerable<Guid> ParseGuids(string guidString)
        {
            if (string.IsNullOrWhiteSpace(guidString)) yield break;
            
            var guids = guidString.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var guid in guids)
            {
                if (TryParseGuid(guid, out var g))
                    yield return g;
            }
        }

        public static bool TryParseGuid(string guidString, out Guid guid)
        {
            try
            {

                guid = Guid.ParseExact(guidString.PadLeft(32, '0'), "N");
                return true;
            }
            catch (FormatException e)
            {
                guid = Guid.Empty;
                return false;
            }
        }
        
        public static Guid GuidParse(string guidString)
        {
            return Guid.ParseExact(guidString.PadLeft(32, '0'), "N");
        }
    }
}
