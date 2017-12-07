using System.Collections.Generic;
using System.Runtime.Serialization;
using FormValidator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
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
            
            _server.Get("/feed/status", async (req, res) =>
            {
                if (!req.Queries.TryGetValue("apikey", out var apiKey))
                {
                    await res.SendString("Invalid request (missing API key)", status: 401);
                    return;
                }
                var server = auth.GetFeedServer(apiKey);
                
                if (server != null)
                    await res.SendJson(server);
                else
                    await res.SendString("Invalid request (invalid API key)", status: 404);

            });
            _server.Post("/feed/update", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var createFeedResult = await fMan.UpdateFeed(form);
                if (server != null)
                    await res.SendJson(server);
                else
                    await res.SendString("Invalid request (invalid API key)", status: 404);

            });
            _server.Post("/feed/create", async (req, res) =>
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
            
            _server.Post("/requestapikey", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var ok = auth.RequestApiKey(form);

            });
        }

        public void Start()
        {
            _server.Start();
        }
    }

    class Authenticator
    {
        private ApiKeyGenerator _keygen;
        private FormValidator.FormValidator _requestFormValidator;
        private FeedRepoDatabase _db;

        public Authenticator(FeedRepoDatabase db)
        {
            _db = db;
            _keygen = new ApiKeyGenerator(db);
            _requestFormValidator = FormValidatorBuilder.New()
                .RequiresString("email")
                .RequiresString("type")
                .RequiresString("name")
                .RequiresString("street")
                .RequiresString("zip", 4)
                .RequiresString("city")
                .RequiresString("country")
                .Build();

        }

        public bool RequestApiKey(IFormCollection form)
        {
            if (!_requestFormValidator.Validate(form))
                return false;
            var customer = new Customer();
            
        }


        public Feed GetFeedServer(StringValues apiKey)
        {
            return _db.Feeds.FindOne(apiKey);
            
        }
    }

    class Customer
    {
        public string Email { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Street { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
