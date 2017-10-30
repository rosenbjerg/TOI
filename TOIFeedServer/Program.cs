using System;
using System.Linq;
using ServiceStack.Text;
using TOIClasses;


namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            JsConfig.UseSystemParseMethods = true;
            var travisBuild = args.Contains("--travis");
            var generateSampleData = args.Contains("--sample-data");
            if (generateSampleData)
            {
                Console.WriteLine("Adding sample tag data.");
            }

            var fs = new FeedServer(generateSampleData);
            fs.Start();

            // Block thread to avoid closing server immediately
            if (!travisBuild)
                Console.ReadLine();
            Console.WriteLine("Server closed");
        }
    }
}
