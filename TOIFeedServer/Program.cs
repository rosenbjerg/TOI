using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RedHttpServerCore;
using TOIClasses;
using TOIFeedServer.Managers;
using TOIFeedServer.Models;


namespace TOIFeedServer
{
    public class Server
    {
        public static void Main(string[] args)
        {
            var travisBuild = args.Contains("--travis");
            var generateSampleData = args.Contains("--sample-data");
            new FeedServer(travisBuild, generateSampleData);
        }
    }

    public class FeedServer
    {
        private readonly RedHttpServer _server = new RedHttpServer(7474);

        public FeedServer(bool travisDeploy, bool sampleData)
        {
            _server.Get("/hello", async (req, res) =>
            {
                await res.SendString("Hello World");
                res.ServerPlugins.Use<DatabaseService>();
            });

            var tagMan = new TagManager();
            _server.Post("/tags", tagMan.AllTags);

            if (sampleData)
            {
                if(File.Exists("toi.db"))
                    File.Delete("toi.db");
                fillMockDatabase(_server);
            }

            _server.ConfigureServices = s => { s.AddDbContext<DatabaseContext>(); };
            _server.Plugins.Register<DatabaseService, DatabaseService>(new DatabaseService());
            _server.Start();
            if (!travisDeploy)
                Console.ReadLine();
        }

        private void fillMockDatabase(RedHttpServer server)
        {
            var modelList = new List<ToiModel>
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
                                TagId = TagManager.CreateTagGuid("FA:C4:D1:03:8D:3D"),
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
                                TagId = TagManager.CreateTagGuid("CC:14:54:01:52:82"),
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
                                TagId = TagManager.CreateTagGuid("CB:FF:B9:6C:A4:7D"),
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
                                TagId = TagManager.CreateTagGuid("F4:B4:15:05:42:05"),
                                TagType = TagType.Bluetooth
                            }
                        }
                    };
            server.Plugins.Use<DatabaseService>().InsertToiModelList(modelList);
        }
    }
}