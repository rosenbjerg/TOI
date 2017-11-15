using System;
using System.Linq;
using ServiceStack.Text;


namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            var help = args.Contains("--help");
            if (help)
            {
                Console.WriteLine("--development\t\tSpecifies that the server runs in development mode, using LiteDB.");
                Console.WriteLine("--sample-data\t\tGenerates some sample data, if no toi already exists.");
                Console.WriteLine("--travis\t\tFor Travis only.");
                Console.ReadLine();
                return;
            }
            
            var travisBuild = args.Contains("--travis");
            var generateSampleData = args.Contains("--sample-data");
            var development = args.Contains("--development");

            if (generateSampleData)
            {
                Console.WriteLine("Adding sample tag data.");
            }

            var fs = new FeedServer(development, generateSampleData);
            fs.Start();
            

            // Block thread to avoid closing server immediately
            if (!travisBuild)
                Console.ReadLine();
            Console.WriteLine("Server closed");
        }
    }
}
