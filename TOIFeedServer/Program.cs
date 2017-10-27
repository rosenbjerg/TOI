using System;
using System.Linq;
using TOIClasses;


namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            var travisBuild = args.Contains("--travis");
            var generateSampleData = args.Contains("--sample-data");
            var fs = new FeedServer(generateSampleData);
            fs.Start();

            // Block thread to avoid closing server immediately
            if (!travisBuild)
                Console.ReadLine();
        }
    }
}