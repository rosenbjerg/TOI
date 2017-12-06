using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using RedHttpServerCore;
using TOIClasses;
using TOIFeedRepo.Database;
using TOIFeedRepo.Managers;
using TOIFeedServer;

namespace TOIFeedRepo
{
    class FeedRepo
    {
        private readonly RedHttpServer _server;

        public FeedRepo(bool development = false, int port = 7575)
        {
            _server = new RedHttpServer(port, "./public");

            _server.Get("hello", async (req, res) =>
            {
                await res.SendString("Hellow from the feed repo!");
            });

            var db = FeedRepoDatabaseFactory.Build(development ? DatabaseFactory.DatabaseType.InMemory : DatabaseFactory.DatabaseType.MongoDB);

            var fMan = new FeedServerManager(db);
            _server.Get("/feeds", async (req, res) =>
            {
                var feeds = await fMan.AllActiveFeeds();
                if (feeds != null)
                {
                    await res.SendJson(feeds);
                }
                else
                {
                    await res.SendJson("No active feeds", StatusCodes.Status204NoContent);
                }
            });
            _server.Post("/feeds/fromlocation", async (req, res) =>
            {
                var location = await req.ParseBodyAsync<GpsLocation>();
                var feeds = await fMan.FeedsFromLocation(location);
                if (feeds != null)
                {
                    await res.SendJson(feeds);
                }
                else
                {
                    await res.SendJson("No active feeds", StatusCodes.Status204NoContent);
                }
            });
            _server.Post("/feed", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var createFeedResult = await fMan.RegisterFeed(form);

                if (createFeedResult.Result != null)
                {
                    await res.SendJson(createFeedResult.Result);
                }
                else
                {
                    await res.SendString(createFeedResult.Message, status: StatusCodes.Status400BadRequest);
                }
            });
        }

        public void Start()
        {
            _server.Start();
        }
    }
}
