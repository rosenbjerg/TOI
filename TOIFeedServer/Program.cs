using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedHttpServerCore;

namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            var service = new DatabaseService();
            var model = new ToiModel("Hejsa");
            var model2 = new ToiModel("Heya");

            var myList = new List<ToiModel>
            {
                model,
                model2
            };

            service.InsertToiModelList(myList);

            var response = service.GetToiModelFromContext("Heya");

            Console.WriteLine(response.ToList().Count);
            Console.ReadLine();

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

            _server.ConfigureServices = s => { s.AddDbContext<ToiModelContext>(); };
            _server.Plugins.Register<DatabaseService, DatabaseService>(new DatabaseService());
            _server.Start("127.0.0.1");
        }
    }
}

