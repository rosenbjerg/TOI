using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RedHttpServerCore;


namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            new FeedServer();

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
            _server.Post("/tags", async (req, res) =>
            {
                List<Guid> ids = await req.ParseBodyAsync<List<Guid>>();
                res.ServerPlugins.Use<DatabaseService>().GetToisByTagIds(ids);
            });


            _server.ConfigureServices = s => { s.AddDbContext<DatabaseContext>(); };
            _server.Plugins.Register<DatabaseService, DatabaseService>(new DatabaseService());
            _server.Start("127.0.0.1");
            Console.ReadKey();
        }
    }
}

