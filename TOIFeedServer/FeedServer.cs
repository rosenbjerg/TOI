using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RedHttpServerCore;
using TOIFeedServer.Database;
using TOIFeedServer.Managers;
using TOIFeedServer.Models;

namespace TOIFeedServer
{
    public class FeedServer
    {
        private readonly RedHttpServer _server;

        public FeedServer(bool sampleData = false, bool testDb = false, int port = 7474)
        {
            _server = !testDb ? new RedHttpServer(port, "./WebManagement") : new RedHttpServer(port);

            _server.Get("/hello", async (req, res) => { await res.SendString("Hello World"); });

            var dbService = new DatabaseService(testDb);
            _server.Plugins.Register<DatabaseService, DatabaseService>(dbService);
            var tagMan = new TagManager(dbService);
            var toiMan = new ToiManager(dbService);

            _server.Post("/tags", async (req, res) =>
            {
                var ids = await req.ParseBodyAsync<HashSet<Guid>>();
                var tags = await tagMan.GetTags(ids);

                if (tags != null)
                {
                    await res.SendJson(tags);
                }
                else
                {
                    await res.SendString("ERROR", status: 400);
                }
            });
            _server.Post("/createtag", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await tagMan.CreateTag(form))
                {
                    await res.SendString("OK");
                }
                else
                {
                    await res.SendString("ERROR", status: 400);
                }
            });
            _server.Post("/updatetag", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await tagMan.UpdateTag(form))
                {
                    await res.SendString("OK");
                }
                else
                {
                    await res.SendString("ERROR", status: 400);
                }
            });
            _server.Get("/getTag", async (req, res) =>
            {
                var tag = await tagMan.GetTag(req.Queries);
                if (tag != null)
                    await res.SendJson(tag);
                else
                    await res.SendString("The tag could not be found.", status: 404);
            });

            _server.Get("/tois", async (req, res) =>
            {
                var contextString = "";
                if (req.Queries.ContainsKey("contexts"))
                    contextString = req.Queries["contexts"][0];
                var tois = await toiMan.GetToisByContext(contextString);
                await res.SendJson(tois);
            });
            
            //TODO implement method for getting a single ToiModel
            _server.Post("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var toiId = await toiMan.CreateToi(form);
                if (toiId != Guid.Empty)
                    await res.SendString(toiId.ToString("N"));
                else
                    await res.SendString("The TOI could not be created.", status: 400);
            });
            _server.Put("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await toiMan.UpdateToi(form))
                    await res.SendString("OK");
                else
                    await res.SendString("The tag could not be updated.", status: 400);
            });
            
            if (sampleData)
            {
                FillMockDatabase();
            }

            _server.ConfigureServices = s => { s.AddDbContext<DatabaseContext>(); };
        }

        private async void FillMockDatabase()
        {
            if (_server.Plugins.Use<DatabaseService>().GetAllToiModels().Result.Status != DatabaseStatusCode.NoElement)
                return;

            var testContext1 = new ContextModel
            {
                Id = Guid.NewGuid(),
                Title = "Grown-up stuff"
            };
            var testContext2 = new ContextModel
            {
                Id = Guid.NewGuid(),
                Title = "For børn"
            };
            var tag1 = new TagModel
            {
                Name = "F-Klubben",
                TagId = TagManager.CreateTagGuid("FA:C4:D1:03:8D:3D"),
                TagType = TagType.Bluetooth
            };
            var tag2 = new TagModel
            {
                Name = "Cassiopeia",
                TagId = TagManager.CreateTagGuid("CC:14:54:01:52:82"),
                TagType = TagType.Bluetooth
            };
            var tag3 = new TagModel
            {
                Name = "At Marius place",
                TagId = TagManager.CreateTagGuid("CB:FF:B9:6C:A4:7D"),
                TagType = TagType.Bluetooth
            };
            var tag4 = new TagModel
            {
                Name = "By the bin",
                TagId = TagManager.CreateTagGuid("F4:B4:15:05:42:05"),
                TagType = TagType.Bluetooth
            };
            
            var modelList = new List<ToiModel>
            {
                new ToiModel
                {
                    Id = Guid.NewGuid(),
                    Description = "FA:C4:D1:03:8D:3D",
                    Title = "Tag 1",
                    Image = "https://i.imgur.com/gCTCL7z.jpg",
                    Url = "https://imgur.com/gallery/yWoZC",
                    ContextModels = { testContext1 },
                    
                    TagModels = new List<TagModel>
                    {
                        tag1, tag2
                    }
                },
                new ToiModel
                {
                    Id = Guid.NewGuid(),
                    Description = "CC:14:54:01:52:82",
                    Title = "Tag 2",
                    Image = "https://i.imgur.com/6UwO2nF.mp4",
                    Url = "https://imgur.com/gallery/6UwO2nF",
                    ContextModels = { testContext1 },
                    TagModels = new List<TagModel>
                    {
                        tag2
                    }
                },
                new ToiModel
                {
                    Id = Guid.NewGuid(),
                    Description = "CB:FF:B9:6C:A4:7D",
                    Title = "Tag 3",
                    Image = "https://i.imgur.com/aNV3gzq.png",
                    Url = "https://imgur.com/gallery/aNV3gzq",
                    ContextModels = { testContext2 },

                    TagModels = new List<TagModel>
                    {
                        tag3
                    }
                },
                new ToiModel
                {
                    Id = Guid.NewGuid(),
                    Description = "F4:B4:15:05:42:05",
                    Title = "Tag 4",
                    Image = "https://i.imgur.com/2Ivtb0i.jpg",
                    Url = "https://gist.github.com/Joklost/7efd0e7b3cafd26ea61b2d7c71961a59",
                    ContextModels = { testContext1, testContext2 },

                    TagModels = new List<TagModel>
                    {
                        tag4
                    }
                }
            };
            await _server.Plugins.Use<DatabaseService>().InsertContexts(new List<ContextModel> {testContext1, testContext2});
            await _server.Plugins.Use<DatabaseService>().InsertToiModelList(modelList);
        }

        public void Start()
        {
            _server.Start();
        }
    }
}