using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceStack.Text;
using TOIFeedServer.Database;
using TOIFeedServer.Managers;
using TOIFeedServer.Models;
using static TOIFeedServer.Extensions;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class ToiManagerTest
    {
        private static ToiManager _manager;
        private const string _tagGuid = "F4B41505420";
        private const string _ctxGuid = "F5B415054200";
        private const string _toiGuid = "F6:B4:15:05:42:0";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var mockDbService = new DatabaseService(true);
            //Insert a mock context for the toi
            var ctxTask = mockDbService.InsertContext(new ContextModel(_ctxGuid, "Mock Context",
                "This is a mock context used for unit testing."));
            ctxTask.Wait();

            //Insert a couple of tags that can be used by the toi
            for (var i = 0; i < 10; i++)
            {
                var tagTask = mockDbService.InsertTag(
                    new TagModel(_tagGuid + i, TagType.Bluetooth)
                );
                tagTask.Wait();
            }

            _manager = new ToiManager(new DatabaseService(true));

            for (var i = 0; i < 3; i++)
            {
                var tags = new List<string>
                {
                    _tagGuid + i
                };
                var form = new FormCollection(new Dictionary<string, StringValues>
                {
                    {"context", _ctxGuid},
                    {"tags", JsonConvert.SerializeObject(tags)},
                    {"url", "https://mock.com" },
                    {"title", "Mock TOI" },
                    {"description", "This is a mock TOI." }
                });

                var task = _manager.CreateToi(form);
                task.Wait();
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
        }

        [TestMethod]
        public void CreateTOI_OneTag_Valid()
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                {"contexts", _ctxGuid},
                {"tags", JsonConvert.SerializeObject(new List<string> {_tagGuid + '0'}) },
                {"url", "https://mock.com" },
                {"title", "Mock TOI" },
                {"description", "This is a mock TOI." }
            });

            var task = _manager.CreateToi(form);
            task.Wait();

            Assert.AreNotEqual(task.Result, Guid.Empty);
        }

        [TestMethod]
        public void CreateTOI_SeveralTags_Valid()
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                {"contexts", _ctxGuid},
                {"tags", JsonConvert.SerializeObject(new List<string> {_tagGuid + '1', _tagGuid + '2', _tagGuid + '3'}) },
                {"url", "https://mock.com" },
                {"title", "Mock TOI" },
                {"description", "This is a mock TOI." }
            });

            var task = _manager.CreateToi(form);
            task.Wait();

            Assert.AreNotEqual(task.Result, Guid.Empty);
        }

        [DataTestMethod]
        [DataRow("", "[" + _tagGuid + "0]", "Mock TOI", "Mock URL", "This is a mock TOI.")]
        [DataRow(_ctxGuid, "", "Mock TOI", "Mock URL", "This is a mock TOI.")]
        [DataRow("F6:B4:15:05:42:99", "[" + _tagGuid + "1]", "Mock TOI", "Mock URL", "This is a mock TOI.")]
        [DataRow(_ctxGuid, "[" + _tagGuid + "0]", "", "Mock URL", "This is a mock TOI.")]
        [DataRow(_ctxGuid, "[" + _tagGuid + "0]", "Mock TOI", "", "This is a mock TOI.")]
        [DataRow(_ctxGuid, "[" + _tagGuid + "0]", "Mock TOI", "Mock URL", "")]
        public void CreateToi__Invalid(string contextId, string tags, string title, string url, string description)
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                {"contexts", contextId},
                {"tags", tags},
                {"title", title },
                {"url", url },
                {"description", description }
            });

            var task = _manager.CreateToi(form);
            task.Wait();

            Assert.AreEqual(task.Result, Guid.Empty);
        }

        [DataTestMethod]
        [DataRow(_toiGuid + "0", _ctxGuid, "[]", "Mock title", "Mock url", "Mock description")]
        [DataRow(_toiGuid + "0", "", "[" + _tagGuid + "0, " + _tagGuid + "1]", "Mock title", "Mock url", "Mock description")]
        [DataRow("F6:B4:15:05:42:99", _ctxGuid, "[" + _tagGuid + "0, " + _tagGuid + "1]", "Mock title", "Mock url", "Mock description")] //Non-existing tag
        [DataRow("", "G4:B4:15:05:42:00", "[" + _tagGuid + "0, " + _tagGuid + "1]", "Mock title", "Mock url", "Mock description")]
        [DataRow(_toiGuid + "0", _ctxGuid, "[" + _tagGuid + "0, " + _tagGuid + "1]", "", "Mock url", "Mock description")]
        [DataRow(_toiGuid + "0", _ctxGuid, "[" + _tagGuid + "0, " + _tagGuid + "1]", "Mock title", "", "Mock description")]
        [DataRow(_toiGuid + "0", _ctxGuid, "[" + _tagGuid + "0, " + _tagGuid + "1]", "Mock title", "Mock url", "")]
        public void UpdateToi__Invalid(string toiId, string contextId, string tags, string title, string url, string description)
        {
            var form = new FormCollection(new Dictionary<string, StringValues>
            {
                {"id", toiId },
                {"contexts", contextId},
                {"tags", tags},
                {"title", title },
                {"url", url },
                {"description", description }
            });

            var task = _manager.UpdateToi(form);
            task.Wait();

            Assert.IsFalse(task.Result);
        }

        public void UpdateToi__Valid()
        {
            //TODO implement this test.
            //var fDict = new Dictionary<string, StringValues>
            //{
            //    {"id", Guid.NewGuid().ToString()},
            //    {"context", _ctxGuid},
            //    {"tags", "[F4:B4:15:05:42:00, F4:B4:15:05:42:01]"},
            //    {"title", "Update toi"},
            //    {"url", "kjsahfr"},
            //    {"description", "description"}
            //};
            //var form = new FormCollection(fDict);
            //var t = _manager.CreateToi(form);
            //t.Wait();

            //fDict["tags"] = "[F4:B4:15:05:42:00, F4:B4:15:05:42:01, F4:B4:15:05:42:01]";
            //var t2 = _manager.UpdateToi(new FormCollection(fDict));
            //t2.Wait();

            //Assert.AreEqual();
        }
    }
}
