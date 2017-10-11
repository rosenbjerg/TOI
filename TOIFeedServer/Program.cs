using System;
using RedHttpServerCore;

namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
           
        }
    }

    public class FeedServer
    {
        private readonly RedHttpServer _server = new RedHttpServer(27115);

        public FeedServer()
        {
            _server.Get("/hello", async (req, res) =>
            {
                await res.SendString("Hello World");
            });

            _server.Start(true);
        }
    }
}

