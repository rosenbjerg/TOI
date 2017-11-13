using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TOIClasses;
using TOIFeedServer.Database;
using TOIFeedServer.Models;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class ModelsTest
    {
        private static DatabaseService _dbs;
        private List<string> _guids;
        private List<TagModel> _tags;
        private List<ContextModel> _contexts;
        private List<ToiModel> _tois;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
        }

        [ClassCleanup]
        public static void Cleanup()
        {
        }

        [TestInitialize]
        public void InitializeTest()
        {
            _dbs = new DatabaseService(true);
            FillMock();
        }

        private void FillMock()
        {
            _guids = new List<string>
            {
                "1",
                "2",
                "3"
            };

            _tags = new List<TagModel>
            {
                new TagModel(_guids[0], TagType.Bluetooth)
                {
                    Name = "test1",
                    Longitude = 45.00,
                    Latitude = 50.00
                },

                new TagModel(_guids[1], TagType.Bluetooth)
                {
                    Name = "test2",
                    Longitude = 40,
                    Latitude = 45
                },

                new TagModel(_guids[2], TagType.Gps)
                {
                    Name = "test3",
                    Longitude = 30,
                    Latitude = 20
                }
            };

            _contexts = new List<ContextModel>
            {
                new ContextModel(_guids[0], "Microsoft Visual Studio", "A tour of the IDE"),
                new ContextModel(_guids[1], "Test", "Test"),
                new ContextModel(_guids[2], "Carlsberg tour")
            };

            _tois = new List<ToiModel>
            {
                new ToiModel
                {
                    Id = _guids[0],
                    Description = "kludder",
                    Title = "Test Title",
                    Image =
                        "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg",
                },
                new ToiModel
                {
                    Id = _guids[1],
                    Description = "kludder",
                    Title = "Test Title",
                    Image =
                        "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg"
                }
            };
        }


        [TestMethod]
        public async Task TagUploaded_CorrectFetch()
        {
            //Act
            await _dbs.InsertTag(_tags[0]);
            var res = await _dbs.GetTagFromId(_guids[0]);

            //Assert
            Assert.AreEqual(typeof(TagModel), res.Result.GetType());
        }

        [TestMethod]
        public async Task TagUploaded_ReturnCorrectType()
        {
            //Act
            await _dbs.InsertTag(_tags[0]);
            var res = await _dbs.GetTagFromId(_guids[0]);

            //Assert
            Assert.AreEqual(TagType.Bluetooth, res.Result.TagType);
        }

        [TestMethod]
        public async Task ReturnCorrectNumberTagType()
        {
            //Arrange

            var collection = new List<TagModel>()
            {
                _tags[0],
                _tags[1],
                _tags[2]
            };

            //Act
            await _dbs.InsertTags(collection);
            var res = _dbs.GetTagsFromType(TagType.Bluetooth);
            res.Wait();

            //Assert
            Assert.AreEqual(2, res.Result.Result.Count());
        }

        [TestMethod]
        public async Task SaveContextModel_CorrectlySavedModel_ModelIsSaved()
        {
            // Arrange 


            // Act
            await _dbs.InsertContext(_contexts[0]);
            var res = await _dbs.GetContextFromId(_guids[0]);

            // Assert
            Assert.AreEqual(_guids[0], res.Result.Id);
        }

        [TestMethod]
        public async Task SaveMultipleContexts_CorrectSavedModel_ModelSaved()
        {
            var collection = new List<ContextModel>()
            {
                _contexts[0],
                _contexts[1],
                _contexts[2]
            };

            // Act
            await _dbs.InsertContexts(collection);
            var res = await _dbs.GetAllContexts();

            // Assert
            Assert.AreEqual(3, res.Result.Count());
        }

        [TestMethod]
        public async Task GetToiFromTagId()
        {
            // Act
            var test = await _dbs.InsertToiModel(_tois[0]);

            var result = await _dbs.GetToisByTagIds(new[] {_guids[0]});

            // Assert
            Assert.AreEqual(DatabaseStatusCode.Ok, result.Status);
            Assert.AreEqual(DatabaseStatusCode.Created, test);
            Assert.AreEqual(1, result.Result.Count());
            var first = result.Result.FirstOrDefault();
            Assert.AreEqual(_guids[0], first?.Tags[0]);
        }

        [TestMethod]
        public async Task InsertToiModelListReturnDataBaseStatusCodeListContainsDuplicates()
        {
            var toiModels = new List<ToiModel>
            {
                _tois[0],
                _tois[0]
            };

            var result = await _dbs.InsertToiModelList(toiModels);

            Assert.AreEqual(DatabaseStatusCode.ListContainsDuplicate, result);
        }

        [TestMethod]
        public async Task GetMultipleToisFromMultipleTags()
        {
            var tags = new List<TagModel>()
            {
                _tags[0],
                _tags[1]
            };
            var tois = new List<ToiModel>()
            {
                _tois[0],
                _tois[1]
            };
            var tagsId = new List<string>()
            {
                _tags[0].TagId,
                _tags[1].TagId
            };


            await _dbs.InsertTags(tags);
            await _dbs.InsertToiModelList(tois);

            var result = await _dbs.GetToisByTagIds(tagsId);

            Assert.AreEqual(true, tois.All(result.Result.Contains));
        }


        [TestMethod]
        public async Task UpdateToI()
        {
            var toi2 = new ToiModel
            {
                Id = _guids[0],
                Description = "test2",
                Title = "test2",
                Url = "test2"
            };
            var toi1 = new ToiModel
            {
                Id = _guids[0],
                Description = "test",
                Title = "test",
                Url = "test"
            };


            var insertStatusCode = await _dbs.InsertToiModel(toi1);

            var statusCode = await _dbs.UpdateToiModel(toi2);

            var updated = await _dbs.GetToi(_guids[0]);

            Assert.AreEqual(DatabaseStatusCode.Created, insertStatusCode);
            Assert.AreEqual(DatabaseStatusCode.Ok, statusCode);
            Assert.AreEqual("test2", updated.Result.Description);
        }

        [TestMethod]
        public async Task UpdateToiCorrectTags()
        {
            var toi1 = _tois[0];
            var tagsBefore = toi1.Tags.Count;
            var insertStatusCode = await _dbs.InsertToiModel(toi1);
            toi1.Tags.Add(_tags[1].TagId);
            var statusCode = await _dbs.UpdateToiModel(toi1);

            var updated = await _dbs.GetToi(toi1.Id);

            Assert.AreEqual(DatabaseStatusCode.Created, insertStatusCode);
            Assert.AreEqual(DatabaseStatusCode.Ok, statusCode);
            Assert.AreEqual(tagsBefore + 1, updated.Result.Tags.Count);
        }
    }
}