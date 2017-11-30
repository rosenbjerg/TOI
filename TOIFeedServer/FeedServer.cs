using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RedHttpServerCore;
using TOIClasses;
using TOIFeedServer.Managers;
using Newtonsoft.Json;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer
{
    public class FeedServer
    {
        private readonly RedHttpServer _server;

        public FeedServer(bool development, bool sampleData = false, int port = 7474)
        {
            var uploadsDir = Path.Combine(".", "public", "uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Console.WriteLine("Creating uploads folder.");
                Directory.CreateDirectory(uploadsDir);
            }

            _server = new RedHttpServer(port, "./public");

            _server.Get("/hello", async (req, res) => { await res.SendString("Hello World"); });

            Console.WriteLine(development ? "Using In-memory db" : "Using MongoDB");

            var db = DatabaseFactory.BuildDatabase(development ? DatabaseFactory.DatabaseType.InMemory : DatabaseFactory.DatabaseType.MongoDB);
            _server.Plugins.Register<Database, Database>(db);
            var tagMan = new TagManager(db);
            var toiMan = new ToiManager(db);
            var usrMan = new UserManager(db);
            var cMan = new ContextManager(db);

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
                    await res.SendJson(tags.Result);
                else
                    await res.SendString("ERROR", status: 400);
            });
            _server.Post("/tag", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var tag = await tagMan.CreateTag(form);
                if (tag.Result != null)
                    await res.SendJson(tag.Result);
                else
                    await res.SendString(tag.Message, status: 400);
            });
            _server.Put("/tag", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var tag = await tagMan.UpdateTag(form);
                if (tag.Result != null)
                    await res.SendJson(tag.Result);
                else
                    await res.SendString(tag.Message, status: 400);
            });
            _server.Get("/tag", async (req, res) =>
            {
                var tag = await tagMan.GetTag(req.Queries);
                if (tag != null)
                    await res.SendJson(tag);
                else
                    await res.SendString("The tag could not be found.", status: StatusCodes.Status404NotFound);
            });
            _server.Delete("/tag", async(req, res) =>
            {
                try
                {
                    var form = await req.GetFormDataAsync();
                    if (await tagMan.DeleteTag(form))
                        await res.SendString("OK");
                    else
                        await res.SendString("The tag could not be deleted.", status: 400);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await res.SendString("ERROR", status: 500);
                }
            });

            _server.Get("/tois", async (req, res) =>
            {
                var contextString = "";
                if (req.Queries.ContainsKey("contexts"))
                    contextString = req.Queries["contexts"][0];
                var tois = await toiMan.GetToisByContext(contextString);
                await res.SendJson(tois.Result);
            });
            _server.Post("/toi/fromtags", async (req, res) =>
            {
                var bString = await req.ParseBodyAsync<string>();
                List<string> tags;
                try
                {
                    tags = JsonConvert.DeserializeObject<List<string>>(bString);
                }
                catch (Exception)
                {
                    await res.SendString("Exception", status: StatusCodes.Status400BadRequest);
                    throw;
                }
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
            _server.Post("/toi/fromgps", async (req, res) =>
            {
                var gpsLocation = await req.ParseBodyAsync<GpsLocation>();
                var toi = await toiMan.GetToiByGpsLocation(gpsLocation);
                if (toi.Status == DatabaseStatusCode.NoElement)
                {
                    await res.SendString("Not found", status: StatusCodes.Status404NotFound);
                }
                else
                {
                    await res.SendJson(toi.Result);
                }
            });
            _server.Post("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var toi = await toiMan.CreateToi(form);
                if (toi.Result != null)
                    await res.SendJson(toi.Result);
                else
                    await res.SendString(toi.Message, status: 400);
            });
            _server.Put("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var toi = await toiMan.UpdateToi(form);
                if (toi.Result != null)
                    await res.SendJson(toi.Result);
                else
                    await res.SendString(toi.Message, status: 400);
            });
            _server.Delete("/toi", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await toiMan.DeleteToi(form))
                    await res.SendString("OK");
                else
                    await res.SendString("The ToI could not be deleted.", status: 400);
            });


            _server.Get("/contexts", async (req, res) =>
            {
                var all = await db.Contexts.GetAll();
                await res.SendJson(all.Result);
            });
            _server.Post("/context", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var ctx = await cMan.CreateContext(form);
                if (ctx.Result != null)
                    await res.SendJson(ctx.Result);
                else
                    await res.SendString(ctx.Message, status: 400);
            });
            _server.Put("/context", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var ctx = await cMan.UpdateContext(form);
                if (ctx.Result != null)
                    await res.SendJson(ctx.Result);
                else
                    await res.SendString(ctx.Message, status: 400);
            });
            _server.Delete("/context", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (await cMan.DeleteContext(form))
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
            if (_server.Plugins.Use<Database>().Tois.GetAll().Result.Status != DatabaseStatusCode.NoElement)
            {
                Console.WriteLine("Sample data already added.");
                return;
            }

            var grownGuid = Guid.NewGuid().ToString("N");
            var childGuid = Guid.NewGuid().ToString("N");
            var grownCtx = new ContextModel
            {
                Id = grownGuid,
                Title = "Grown-up stuff",
                Description = "Marks legetøj"
            };
            var childCtx = new ContextModel
            {
                Id = childGuid,
                Title = "For børn"
            };
            var fTag = new TagModel
            {
                Title = "F-Klubben",
                Id = "FAC4D1038D3D",
                Type = TagType.Nfc
            };
            var cTag = new TagModel
            {
                Title = "Cassiopeia",
                Id = "CC1454015282",
                Type = TagType.Bluetooth
            };
            var mTag = new TagModel
            {
                Title = "At Marius place",
                Id = "CBFFB96CA47D",
                Type = TagType.Wifi
            };
            var btbTag = new TagModel
            {
                Title = "By the bin",
                Id = "F4B415054205",
                Type = TagType.Gps
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
                    Tags = new List<string> {mTag.Id},
                    InformationType = ToiInformationType.Website
                },
                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "Cocio and Tekken!",
                    Title = "F-klubben",
                    Image = "http://i36.tinypic.com/2e5jdsk.jpg",
                    Url = "https://imgur.com/gallery/6UwO2nF",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {fTag.Id},
                    InformationType = ToiInformationType.Video
                },
                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "A place where nerds go daily to study computers. Smells a bit like burnt leather and horseblanket.",
                    Title = "Cassiopeia",
                    Image = "https://i.imgur.com/aNV3gzq.png",
                    Url = "https://imgur.com/gallery/aNV3gzq",
                    Contexts = new List<string> {childCtx.Id},
                    Tags = new List<string> {cTag.Id},
                    InformationType = ToiInformationType.Text
                },
                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Description = "This hosts many interesting items that have been disposed during the week. If it stands untouched too long it will deteriorate into a pile of smelly goo.",
                    Title = "Out Scraldespand",
                    Image = "https://i.imgur.com/2Ivtb0i.jpg",
                    Url = "https://gist.github.com/Joklost/7efd0e7b3cafd26ea61b2d7c71961a59",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {btbTag.Id},
                    InformationType = ToiInformationType.Text
                },

                new ToiModel
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Title = "AAU",
                    Description = "Massive party at AAU. DEM gurlws are hoot!",
                    Image = "https://i5.walmartimages.com/asr/fa1be18a-e37d-4387-b6bd-3c4fba36e1fa_1.a6268444b1193d23137622d8ff7c58b4.jpeg",
                    Url = "http://www.fklub.dk/",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {cTag.Id, btbTag.Id, mTag.Id, fTag.Id},
                    InformationType = ToiInformationType.Website
                }
            };

            var db = _server.Plugins.Use<Database>();
            await db.Tags.Insert(cTag, btbTag, mTag, fTag);
            await db.Contexts.Insert(grownCtx, childCtx);
            await db.Tois.Insert(modelList.ToArray());
        }

        public void Start()
        {
            _server.Start();
        }
    }
}