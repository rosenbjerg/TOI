using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RedHttpServerCore;
using TOIClasses;
using TOIFeedServer.Models;


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
                List<Guid> ids;
                //try
                //{
                //    ids = await req.ParseBodyAsync<List<Guid>>();
                //}
                //catch
                //{
                //    await res.SendString("NO", status: 401);
                //    return;
                //}
                var tag = new TagModel(Guid.ParseExact("cc1454015282".PadLeft(32, '0'), "N"), TagType.Bluetooth);
                var toi = new ToiModel(Guid.NewGuid(), new TagInfoModel{Description = "the quick brown fox jumps over the lazy dog ", Title = "Test Title", Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg" })
                {
                    TagModel = tag
                };
                List<TagInfo> testList = new List<TagInfo>()
                {
                    toi.Info.GetTagInfo()
                }; 
                await res.SendJson(testList);
            });


            _server.ConfigureServices = s => { s.AddDbContext<DatabaseContext>(); };
            _server.Plugins.Register<DatabaseService, DatabaseService>(new DatabaseService());
            _server.Start();
        }
    }
}

