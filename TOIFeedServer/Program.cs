using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                //                List<Guid> ids;
                //                try
                //                {
                //                    ids = await req.ParseBodyAsync<List<Guid>>();
                //                }
                //                catch
                //                {
                //                    await res.SendString("NO", status: 401);
                //                    return;
                //                }
                //
                //                var outList = new List<TagInfo>();
                //                ids.ForEach(tag => outList.Add(_server.Plugins.Use<DatabaseService>().GetToisByTagId(tag).TagInfoModel.GetTagInfo()));

                var modelList = 
                    new List<ToiModel>
                    {
                        new ToiModel
                        {
                            Id = Guid.NewGuid(),
                            TagInfoModel = new TagInfoModel
                            {
                                Description = "FA:C4:D1:03:8D:3D",
                                Title = "Tag 1",
                                Image = "https://i.imgur.com/gCTCL7z.jpg",
                                Url = "https://imgur.com/gallery/yWoZC"
                            },
                            TagModel = new TagModel
                            {
                                TagId = CreateGuid("FA:C4:D1:03:8D:3D"),
                                TagType = TagType.Bluetooth
                            }
                        },
                        new ToiModel
                        {
                            Id = Guid.NewGuid(),
                            TagInfoModel = new TagInfoModel
                            {
                                Description = "CC:14:54:01:52:82",
                                Title = "Tag 2",
                                Image = "https://i.imgur.com/6UwO2nF.mp4",
                                Url = "https://imgur.com/gallery/6UwO2nF"
                            },
                            TagModel = new TagModel
                            {
                                TagId = CreateGuid("CC:14:54:01:52:82"),
                                TagType = TagType.Bluetooth
                            }
                        },
                        new ToiModel
                        {
                            Id = Guid.NewGuid(),
                            TagInfoModel = new TagInfoModel
                            {
                                Description = "CB:FF:B9:6C:A4:7D",
                                Title = "Tag 3",
                                Image = "https://i.imgur.com/aNV3gzq.png",
                                Url = "https://imgur.com/gallery/aNV3gzq"
                            },
                            TagModel = new TagModel
                            {
                                TagId = CreateGuid("CB:FF:B9:6C:A4:7D"),
                                TagType = TagType.Bluetooth
                            }
                        },
                        new ToiModel
                        {
                            Id = Guid.NewGuid(),
                            TagInfoModel = new TagInfoModel
                            {
                                Description = "F4:B4:15:05:42:05",
                                Title = "Tag 4",
                                Image = "https://i.imgur.com/2Ivtb0i.jpg",
                                Url = "https://gist.github.com/Joklost/7efd0e7b3cafd26ea61b2d7c71961a59"
                            },
                            TagModel = new TagModel
                            {
                                TagId = CreateGuid("F4:B4:15:05:42:05"),
                                TagType = TagType.Bluetooth
                            }
                        }
                    };
                res.ServerPlugins.Use<DatabaseService>().InsertToiModelList(modelList);
                
                var tagInfoList = new List<TagInfo>();
                res.ServerPlugins.Use<DatabaseService>().GetAllToiModels().ToList().ForEach(p => tagInfoList.Add(p.TagInfoModel.GetTagInfo()));
                await res.SendJson(tagInfoList);
                res.ServerPlugins.Use<DatabaseService>().TruncateDatabase();
            });

            if (File.Exists("toi.db"))
            {
                File.Delete("toi.db");
            }

            _server.ConfigureServices = s => { s.AddDbContext<DatabaseContext>(); };
            _server.Plugins.Register<DatabaseService, DatabaseService>(new DatabaseService());
            _server.Start();
            Console.ReadKey();
        }

        public Guid CreateGuid(string bdAddr)
        {
            return Guid.ParseExact(bdAddr.Replace(":", string.Empty).PadLeft(32, '0'), "N");
        }
    }
}