using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedHttpServerCore;
using TOIFeedServer.Models;

namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            var service = new DatabaseService();
        }
    }

    public class FeedServer
    {
        private readonly RedHttpServer _server = new RedHttpServer(7474);

        public FeedServer()
        {
            _server.Get("/hello", async (req, res) =>
            {
                await res.SendString("Hello World");
                res.ServerPlugins.Use<DatabaseService>();
            });

            _server.ConfigureServices = s => { s.AddDbContext<DatabaseContext>(); };
            _server.Plugins.Register<DatabaseService, DatabaseService>(new DatabaseService());
            _server.Start("127.0.0.1");
        }
    }
}

