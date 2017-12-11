using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http;
using RedHttpServerCore;
using TOIClasses;
using TOIFeedServer.Managers;
using Newtonsoft.Json;
using RedHttpServerCore.Request;
using RedHttpServerCore.Response;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer
{
    public class FeedServer
    {
        private readonly RedHttpServer _server;
        private const string FeedRepo = "http://ssh.windelborg.info:7575/";

        public FeedServer(bool development, bool sampleData = false, int port = 7474)
        {
            _server = new RedHttpServer(port, "./public");

            _server.Get("/hello", async (req, res) => { await res.SendString("Hello World"); });

            Console.WriteLine(development ? "Using In-memory db" : "Using MongoDB");

            var db = DatabaseFactory.BuildDatabase(development ? DatabaseFactory.DatabaseType.InMemory : DatabaseFactory.DatabaseType.MongoDB);
            _server.Plugins.Register<Database, Database>(db);
            var tagMan = new TagManager(db);
            var toiMan = new ToiManager(db);
            var usrMan = new UserManager(db);
            var cMan = new ContextManager(db);
            var fMan = new StaticFileManager(db);
            var httpClient = new HttpClient();

            async Task<bool> CheckAuthentication(RRequest req, RResponse res)
            {
                if (!req.Cookies.ContainsKey("token"))
                {
                    await res.SendString("Unauthorized", status: StatusCodes.Status401Unauthorized);
                    return false;
                }

                var token = req.Cookies["token"];

                if (usrMan.VerifyToken(token))
                    return true;
                await res.SendString("Unauthorized", status: StatusCodes.Status401Unauthorized);
                return false;
            }

            _server.Get("/tags", async (req, res) =>
            {
                var tags = await tagMan.GetTags(req.Queries);

                if (tags != null)
                    await res.SendJson(tags.Result);
                else
                    await res.SendString("ERROR", status: 400);
            });
            _server.Post("/tag", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var tag = await tagMan.CreateTag(form);
                if (tag.Result != null)
                    await res.SendJson(tag.Result);
                else
                    await res.SendString(tag.Message, status: 400);
            });
            _server.Put("/tag", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var tag = await tagMan.UpdateTag(form);
                if (tag.Result != null)
                    await res.SendJson(tag.Result);
                else
                    await res.SendString(tag.Message, status: 400);
            });
            //_server.Get("/tag", async (req, res) =>
            //{
            //    var tag = await tagMan.GetTag(req.Queries);
            //    if (tag != null)
            //        await res.SendJson(tag);
            //    else
            //        await res.SendString("The tag could not be found.", status: StatusCodes.Status404NotFound);
            //});
            _server.Delete("/tag", async(req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

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
                catch (Exception e)
                {
                    await res.SendString("Exception", status: StatusCodes.Status500InternalServerError);
                    Console.WriteLine(e);
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
            _server.Post("/toi", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var toi = await toiMan.CreateToi(form);
                if (toi.Result != null)
                    await res.SendJson(toi.Result);
                else
                    await res.SendString(toi.Message, status: 400);
            });
            _server.Put("/toi", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var toi = await toiMan.UpdateToi(form);
                if (toi.Result != null)
                    await res.SendJson(toi.Result);
                else
                    await res.SendString(toi.Message, status: 400);
            });
            _server.Delete("/toi", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

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
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var ctx = await cMan.CreateContext(form);
                if (ctx.Result != null)
                    await res.SendJson(ctx.Result);
                else
                    await res.SendString(ctx.Message, status: 400);
            });
            _server.Put("/context", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var ctx = await cMan.UpdateContext(form);
                if (ctx.Result != null)
                    await res.SendJson(ctx.Result);
                else
                    await res.SendString(ctx.Message, status: 400);
            });
            _server.Delete("/context", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                if (await cMan.DeleteContext(form))
                    await res.SendString("OK");
                else
                    await res.SendString("The context could not be deleted.", status: 400);
            });

            _server.Get("/files", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var all = await fMan.AllStaticFiles();
                await res.SendJson(all);
            });
            _server.Post("/files", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var fileUploads = await fMan.UploadFiles(form);
                await res.SendJson(fileUploads);
            });
            _server.Put("/files", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var succeeded = await fMan.UpdateStaticFile(form);
                if (succeeded.Result == null)
                {
                    await res.SendString(succeeded.Message, status: StatusCodes.Status400BadRequest);
                }
                else
                {
                    await res.SendJson(succeeded);
                }
            });
            _server.Delete("/files", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();
                var deleted = await fMan.DeleteStaticFile(form);
                if (deleted.Result)
                {
                    await res.SendString(deleted.Message);
                }
                else
                {
                    await res.SendString(deleted.Message, status: StatusCodes.Status400BadRequest);
                }
            });

            _server.Post("/login", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                if (!form.ContainsKey("username") || !form.ContainsKey("password"))
                {
                    await res.SendString("Either the username or password is missing", status: StatusCodes.Status400BadRequest);
                    return;
                }

                var loggedIn = await usrMan.Login(form["username"][0], form["password"][0]);
                if (!loggedIn.Result)
                {
                    await res.SendString(loggedIn.Message, status: StatusCodes.Status401Unauthorized);
                }
                else
                {
                    res.AddHeader("Set-Cookie", loggedIn.Message);

                    try
                    {
                        var feedInfo = await httpClient.GetAsync(FeedRepo + "feed?apiKey=" + db.ApiKey);
                        if (feedInfo.IsSuccessStatusCode)
                        {
                            var feedInfoStr = await feedInfo.Content.ReadAsStringAsync();
                            await res.SendString(feedInfoStr);
                        }
                        else if(feedInfo.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            await res.SendString("The feed server has not been activated yet",
                                status: StatusCodes.Status207MultiStatus);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to connect to Feed Repo");
                        Console.WriteLine(e);
                    }
                }
            });
            _server.Post("/register", async (req, res) =>
            {
                if (!await CheckAuthentication(req, res)) return;

                var form = await req.GetFormDataAsync();

                var created = await usrMan.CreateUser(form);
                if (created.Result != null)
                {
                    var user = created.Result;
                    var token = await usrMan.Login(user.Username, user.Password);
                    res.AddHeader("Set-Cookie", token.Message);
                    await res.SendString(created.Message);
                }
                else
                {
                    await res.SendString(created.Message, status: StatusCodes.Status400BadRequest);
                }
            });

            _server.Put("/feed/location", async (req, res) =>
            {
                if (string.IsNullOrEmpty(db.ApiKey))
                {
                    await res.SendString("The feed server has not been registered yet.", status: 400);
                    return;
                }

                var form = await req.GetFormDataAsync();
                var form2 = form
                    .Select(f => new KeyValuePair<string, string>(f.Key, f.Value[0]))
                    .Append(new KeyValuePair<string, string>("apiKey", db.ApiKey));

                var frRes = await httpClient.PutAsync(FeedRepo + "feed/location", new FormUrlEncodedContent(form2));
                var frBody = await frRes.Content.ReadAsStringAsync();

                if (frRes.IsSuccessStatusCode)
                {
                    await res.SendString(frBody);
                }
                else
                {
                    await res.SendString(frBody, status: 400);
                }
            });
            _server.Post("/feed/deativate", async (req, res) =>
            {
                if (string.IsNullOrEmpty(db.ApiKey))
                {
                    await res.SendString("The feed server has not been registered yet.", status: 400);
                    return;
                }

                var form = new Dictionary<string, string>
                {
                    {"active", "false"},
                    {"apiKey", db.ApiKey }
                };
                
                var frRes = await httpClient.PutAsync(FeedRepo + "feed/active", new FormUrlEncodedContent(form));
                var frBody = await frRes.Content.ReadAsStringAsync();

                if (frRes.IsSuccessStatusCode)
                {
                    await res.SendString(frBody);
                }
                else
                {
                    await res.SendString(frBody, status: 400);
                }
            });
            _server.Post("/feed/activate", async (req, res) =>
            {
                if (string.IsNullOrEmpty(db.ApiKey))
                {
                    await res.SendString("The feed server has not been registered yet.", status: 400);
                    return;
                }

                var form = new Dictionary<string, string>
                {
                    {"active", "true"},
                    {"apiKey", db.ApiKey }
                };

                var frRes = await httpClient.PutAsync(FeedRepo + "feed/active", new FormUrlEncodedContent(form));
                var frBody = await frRes.Content.ReadAsStringAsync();

                if (frRes.IsSuccessStatusCode)
                {
                    await res.SendString(frBody);
                }
                else
                {
                    await res.SendString(frBody, status: 400);
                }
            });
            _server.Put("/feed", async (req, res) =>
            {
                if (string.IsNullOrEmpty(db.ApiKey))
                {
                    await res.SendString("The feed server has not been registered yet.", status: 400);
                    return;
                }

                var form = await req.GetFormDataAsync();
                var form2 = form
                    .Select(f => new KeyValuePair<string, string>(f.Key, f.Value[0]))
                    .Append(new KeyValuePair<string, string>("apiKey", db.ApiKey));

                var frRes = await httpClient.PutAsync(FeedRepo + "feed", new FormUrlEncodedContent(form2));
                var frBody = await frRes.Content.ReadAsStringAsync();

                if (frRes.IsSuccessStatusCode)
                {
                    await res.SendString(frBody);
                }
                else
                {
                    await res.SendString(frBody, status: 400);
                }
            });
            _server.Post("/feed/register", async (req, res) =>
            {
                var form = await req.GetFormDataAsync();
                var frForm = form.Select(f => new KeyValuePair<string, string>(f.Key, f.Value[0]));

                var fRes = await httpClient.PostAsync(FeedRepo + "feed/register", new FormUrlEncodedContent(frForm));
                if (fRes.IsSuccessStatusCode)
                {
                    var feedInfo =
                        JsonConvert.DeserializeObject<UserActionResponse<Feed>>(await fRes.Content.ReadAsStringAsync());

                    await db.StoreApiKey(feedInfo.Result.Id);
                    await res.SendJson(feedInfo.Result);
                }
                else
                {
                    await res.SendString(await fRes.Content.ReadAsStringAsync(), status: (int) fRes.StatusCode);
                }

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
                Type = TagType.Nfc,
                Latitude = 57,
                Longitude = 9.9,
                Radius = 1
            };
            var cTag = new TagModel
            {
                Title = "Cassiopeia",
                Id = "CC1454015282",
                Type = TagType.Gps,
                Latitude = 57,
                Longitude = 9.9,
                Radius = 5
            };
            var mTag = new TagModel
            {
                Title = "At Marius place",
                Id = "CBFFB96CA47D",
                Type = TagType.Wifi,
                Latitude = 57.01608357948024,
                Longitude = 9.985376190429633,
                Radius = 50
            };
            var bmdcTag = new TagModel
            {
                Title = "Business Model Design Center",
                Id = "F4B415054205",
                Type = TagType.Gps,
                Latitude = 57.01794675337907,
                Longitude = 9.97747976708979,
                Radius = 10
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
                    Description = "Yea! You can train your creativity.",
                    Title = "Academy for Creativity",
                    Image = "http://academyforcreativity.com/wp-content/uploads/2017/04/my-logo@2x-1.png-1-e1493053107456.png",
                    Url = "https://academyforcreativity.com",
                    Contexts = new List<string> {grownCtx.Id},
                    Tags = new List<string> {bmdcTag.Id},
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
                    Tags = new List<string> {cTag.Id, bmdcTag.Id, mTag.Id, fTag.Id},
                    InformationType = ToiInformationType.Website
                }
            };

            var db = _server.Plugins.Use<Database>();
            await db.Tags.Insert(cTag, bmdcTag, mTag, fTag);
            await db.Contexts.Insert(grownCtx, childCtx);
            await db.Tois.Insert(modelList.ToArray());
            await db.Users.Insert(new User
            {
                Id = Guid.NewGuid().ToString("N"),
                Username = "sw706",
                Email = "sw706@cs.aau.dk",
                Password = BCrypt.Net.BCrypt.HashPassword("Cocio")
            });
        }

        public void Start()
        {
            _server.Start();
        }
    }
}