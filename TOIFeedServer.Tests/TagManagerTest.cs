using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedHttpServerCore.Request;
using TOIFeedServer.Database;
using TOIFeedServer.Managers;
using TOIClasses;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class TagManagerTest
    {
        private static TagManager _manager;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var dbs = new DatabaseService(DatabaseFactory.DatabaseType.InMemory);
            dbs.TruncateDatabase().Wait();
            _manager = new TagManager(dbs);

            for (var i = 0; i < 10; i++)
            {
                var form = new FormCollection(new Dictionary<string, StringValues>
                {
                    {"title", $"Bluetooth Tag {i}"},
                    {"id", $"F4:B4:15:05:42:0{i}"},
                    {"type", "0"},
                    {"radius", $"3{i}{i}"},
                    {"latitude", $"57.012392{1}"},
                    {"longitude", $"9.991556{i}"}
                });

                var task = _manager.CreateTag(form);
                task.Wait();
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
        }

        [TestMethod]
        public void GetTags_ValidId_ValidTagInfo()
        {
            var tagNo = 3;
            var q = new QueryCollection(new Dictionary<string, StringValues>
            {
                {"id", $"F4:B4:15:05:42:0{tagNo}"}
            });
            var tTask = _manager.GetTag(q);
            tTask.Wait();

            var tag = tTask.Result;
            Assert.IsNotNull(tag);
            Assert.AreEqual($"Bluetooth Tag {tagNo}", tag.Name);
        }

        [TestMethod]
        public void GetTags_InvalidId_Null()
        {
            var tagNo = 999;
            var q = new QueryCollection(new Dictionary<string, StringValues>
            {
                {"id", $"F4:B4:15:05:42:0{tagNo}"}
            });
            var tTask = _manager.GetTag(q);
            tTask.Wait();

            var tag = tTask.Result;
            Assert.IsNull(tag);
        }

        [TestMethod]
        public void CreateTag_AllInput_Valid()
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                {"title", "Bluetooth Tag"},
                {"id", "F4:B4:15:05:43:05"},
                {"type", "0"},
                {"radius", "300"},
                {"latitude", "57.0123920"},
                {"longitude", "9.9915560"}
            });

            var task = _manager.CreateTag(form);
            task.Wait();

            Assert.IsTrue(task.Result);
        }

        [DataTestMethod]
        [DataRow("", "FA:C4:D1:03:8D:3D", "1", "300", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "", "3", "300", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "FB:C4:D1:03:8D:3D", "", "300", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "FC:C4:D1:03:8D:3D", "3", "", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "FD:C4:D1:03:8D:3D", "2", "300", "", "9.9915560")]
        [DataRow("Bluetooth Tag", "FE:C4:D1:03:8D:3D", "3", "300", "57.0123920", "")]
        [DataRow("Bluetooth Tag", "FF:C4:D1:03:8D:3D", "string_type", "300", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "F1:C4:D1:03:8D:3D", "3", "0", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "F2:C4:D1:03:8D:3D", "1", "100.00", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "F3:C4:D1:03:8D:3D", "2", "wrong_radius", "57.0123920", "9.9915560")]
        [DataRow("Bluetooth Tag", "F4:C4:D1:03:8D:3D", "1", "300", "wrong_latitude", "9.9915560")]
        [DataRow("Bluetooth Tag", "F5:C4:D1:03:8D:3D", "2", "300", "57.0123920", "wrong_longitude")]
        public void CreateTag__Invalid(string title, string id, string type, string radius, string latitude, string longitude)
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                {"title", title},
                {"id", id},
                {"type", type},
                {"radius", radius},
                {"latitude", latitude},
                {"longitude", longitude}
            });

            var task = _manager.CreateTag(form);
            task.Wait();

            Assert.IsFalse(task.Result);
        }
    }
}