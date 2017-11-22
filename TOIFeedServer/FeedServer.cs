﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RedHttpServerCore;
using TOIFeedServer.Database;
using TOIFeedServer.Managers;
using TOIFeedServer.Models;
using Newtonsoft.Json;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer
{
    public class FeedServer
    {
        private readonly RedHttpServer _server;

        public FeedServer(bool development, bool sampleData = false, int port = 7474)
        {
            _server = new RedHttpServer(port, "./WebManagement");

            _server.Get("/hello", async (req, res) => { await res.SendString("Hello World"); });

            Console.WriteLine(development ? "Using In-memory db" : "Using MongoDB");

            var dbService = new DatabaseService(development ? DatabaseFactory.DatabaseType.InMemory : DatabaseFactory.DatabaseType.MongoDB);
            _server.Plugins.Register<DatabaseService, DatabaseService>(dbService);
            var tagMan = new TagManager(dbService);
            var toiMan = new ToiManager(dbService);

            _server.Get("/tags", async (req, res) =>
            {
                string ids = null;
                if (req.Queries.ContainsKey("ids"))
                {
                    ids = req.Queries["ids"][0];
                }
                var tagFilter = ids != null ? SplitIds(ids).ToHashSet() : null;
                var tags = await tagMan.GetTags(tagFilter);

                if (tags != null)
                {
                    await res.SendJson(tags);
                }
                else
                {
                    await res.SendString("ERROR", status: 400);
                }
            });
            _server.Post("/tag", async (req, res) =>
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
            _server.Put("/tag", async (req, res) =>
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
            _server.Get("/tag", async (req, res) =>
            {
                var tag = await tagMan.GetTag(req.Queries);
                if (tag != null)
                    await res.SendJson(tag);
                else
                    await res.SendString("The tag could not be found.", status: StatusCodes.Status404NotFound);
            });

            _server.Get("/tois", async (req, res) =>
            {
                var contextString = "";
                if (req.Queries.ContainsKey("contexts"))
                    contextString = req.Queries["contexts"][0];
                var tois = await toiMan.GetToisByContext(contextString);
                await res.SendJson(tois);
            });

            _server.Post("/toi/fromtags", async (req, res) =>
            {
                var bString = await req.ParseBodyAsync<string>();
                var tags = JsonConvert.DeserializeObject<IEnumerable<string>>(bString);
                if (tags == null)
                {
                    await res.SendString("Bad request", status: StatusCodes.Status400BadRequest);
                    return;
                }
                var toi = await toiMan.GetToiByTagIds(tags);
                if (toi.Status == DatabaseStatusCode.NoElement)
                {
                    await res.SendString("Not found", status: StatusCodes.Status404NotFound);
                }
                else
                {
                    await res.SendJson(toi.Result);
                }
            });
            
            _server.Get("/toi", async (req, res) =>
            {
                //TODO implement method for getting a single ToiModel
            });
            _server.Post("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var toiId = await toiMan.CreateToi(form);
                if (toiId != "-1")
                    await res.SendString(toiId);
                else
                    await res.SendString("The ToI could not be created.", status: 400);
            });
            _server.Put("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await toiMan.UpdateToi(form))
                    await res.SendString("OK");
                else
                    await res.SendString("The ToI could not be updated.", status: 400);
            });
            
            _server.Get("/contexts", async (req, res) =>
            {
                var all = await dbService.GetAllContexts();
                await res.SendJson(all);
            }); 
            _server.Post("/context", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var toiId = await toiMan.CreateContext(form);
                if (toiId != "-1")
                    await res.SendString(toiId);
                else
                    await res.SendString("The context could not be created.", status: 400);
            });
            _server.Put("/context", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await toiMan.UpdateContext(form))
                    await res.SendString("OK");
                else
                    await res.SendString("The context could not be updated.", status: 400);
            });
            _server.Delete("/context", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await toiMan.DeleteContext(form))
                    await res.SendString("OK");
                else
                    await res.SendString("The context could not be deleted.", status: 400);
            });
           
            if (sampleData)
            {
                FillMockDatabase();
            }
        }

        private async void FillMockDatabase()
        {
            if (_server.Plugins.Use<DatabaseService>().GetAllToiModels().Result.Status != DatabaseStatusCode.NoElement)
            {
                await _server.Plugins.Use<DatabaseService>().TruncateDatabase();
            }

            var grownGuid = Guid.NewGuid().ToString("N");
            var childGuid = Guid.NewGuid().ToString("N");
            var grownCtx = new ContextModel
            {
                Id = grownGuid,
                Title = "Grown-up stuff"
            };
            var childCtx = new ContextModel
            {
                Id = childGuid,
                Title = "For børn"
            };
            var fTag = new TagModel
            {
                Name = "F-Klubben",
                Id = "FAC4D1038D3D",
                TagType = TagType.Nfc
            };
            var cTag = new TagModel
            {
                Name = "Cassiopeia",
                Id = "CC1454015282",
                TagType = TagType.Bluetooth
            };
            var mTag = new TagModel
            {
                Name = "At Marius place",
                Id = "CBFFB96CA47D",
                TagType = TagType.Wifi
            };
            var btbTag = new TagModel
            {
                Name = "By the bin",
                Id = "F4B415054205",
                TagType = TagType.Gps
            };
            
            var modelList = new List<ToiModel>
            {
                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "Marius appartment is a place for people to meet and play Dungeons and Dragons. These people drink massive amounts of Monster.",
                    Title = "The DND dungeon",
                    Image = "https://i.imgur.com/gCTCL7z.jpg",
                    Url = "https://imgur.com/gallery/yWoZC",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {mTag.Id}
                },
                new ToiModel 
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "Cocio and Tekken!",
                    Title = "F-klubben",
                    Image = "http://i36.tinypic.com/2e5jdsk.jpg",
                    Url = "https://imgur.com/gallery/6UwO2nF",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {fTag.Id}
                },
                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "A place where nerds go daily to study computers. Smells a bit like burnt leather and horseblanket.",
                    Title = "Cassiopeia",
                    Image = "https://i.imgur.com/aNV3gzq.png",
                    Url = "https://imgur.com/gallery/aNV3gzq",
                    Contexts = new List<string> {childCtx.Id},
                    Tags = new List<string> {cTag.Id}
                },
                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "This hosts many interesting items that have been disposed during the week. If it stands untouched too long it will deteriorate into a pile of smelly goo.",
                    Title = "Out Scraldespand",
                    Image = "https://i.imgur.com/2Ivtb0i.jpg",
                    Url = "https://gist.github.com/Joklost/7efd0e7b3cafd26ea61b2d7c71961a59",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {btbTag.Id}
                },

                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Title = "AAU",
                    Description = "Massive party at AAU. DEM gurlws are hoot!",
                    Image = "https://i5.walmartimages.com/asr/fa1be18a-e37d-4387-b6bd-3c4fba36e1fa_1.a6268444b1193d23137622d8ff7c58b4.jpeg",
                    Url = "pornhub.com",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {cTag.Id, btbTag.Id, mTag.Id, fTag.Id}
                }
            };

            var db = _server.Plugins.Use<DatabaseService>();
            await db.InsertTag(cTag, btbTag, mTag, fTag);
            await db.InsertContext(grownCtx, childCtx);
            await db.InsertToiModel(modelList.ToArray());
        }

        public void Start()
        {
            _server.Start();
        }
    }
}