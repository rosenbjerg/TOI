using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using RedHttpServerCore;
using TOIClasses;
using TOIFeedRepo.Database;
using TOIFeedRepo.Managers;
using TOIFeedServer;
using TOIFeedServer.Managers;

namespace TOIFeedRepo
{
    class FeedRepo
    {
        private readonly RedHttpServer _server;

        public FeedRepo(bool development = false, int port = 7575)
        {
            _server = new RedHttpServer(port, "./public");

            var db = FeedRepoDatabaseFactory.Build(development 
                ? DatabaseFactory.DatabaseType.InMemory 
                : DatabaseFactory.DatabaseType.MongoDB);

            var fMan = new FeedServerManager(db);
            var auth = new Authenticator(db);
            
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
            _server.Get("/feed", async (req, res) =>
            {

                if (!req.Queries.ContainsKey("apiKey") || string.IsNullOrEmpty(req.Queries["apiKey"][0]))
                {
                    await res.SendString("Please supply an API key", status: StatusCodes.Status401Unauthorized);
                }
                var id = req.Queries["apiKey"][0];

                var feed = await fMan.GetFeedServer(id);
                if (feed == null)
                    await res.SendString("Invalid ApiKey", status: 401);
                else
                    await res.SendJson(feed);
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

            _server.Put("/feed/update", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var createFeedResult = await fMan.UpdateFeed(form);
                if (createFeedResult.Result != null)
                    await res.SendJson(createFeedResult);
                else
                    await res.SendString(createFeedResult.Message, status: 404);
            });
            _server.Put("/feed/active", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var updateResult = await fMan.UpdateActivation(form);
                if (updateResult.Result != null)
                    await res.SendJson(updateResult);
                else
                {
                    await res.SendJson(updateResult.Message, 400);
                }
            });
            _server.Put("/feed/location", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var updateResult = await fMan.UpdatePosition(form);
                if (updateResult.Result != null)
                    await res.SendJson(updateResult);
                else
                {
                    await res.SendJson(updateResult.Message, 400);
                }
            });

            _server.Put("/feed/register", async (req, res) =>
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
