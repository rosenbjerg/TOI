using System;
using System.Collections.Generic;
using System.Text;
using RedHttpServerCore;

namespace TOIFeedRepo
{
    class FeedRepo
    {
        private readonly RedHttpServer _server;

        public FeedRepo(int port = 7575)
        {
            _server = new RedHttpServer(port, "./public");

            _server.Get("hello", async (req, res) =>
            {
                await res.SendString("Hellow from the feed repo!");
            });
        }

        public void Start()
        {
            _server.Start();
        }
    }
}
