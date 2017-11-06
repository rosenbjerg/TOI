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
        private List<Guid> _guids;
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
            _guids = new List<Guid>
            {
                Guid.ParseExact("1".PadLeft(32, '0'), "N"),
                Guid.ParseExact("2".PadLeft(32, '0'), "N"),
                Guid.ParseExact("3".PadLeft(32, '0'), "N")
            };

            _tags = new List<TagModel>
            {
                new TagModel(_guids[0], TagType.Bluetooth)
                {
                    Name = "test1",

                    Longtitude = 45.00,
                    Latitude = 50.00
                },

                new TagModel(_guids[1], TagType.Bluetooth)
                {
                    Name = "test2",
                    Longtitude = 40,
                    Latitude = 45
                },

                new TagModel(_guids[2], TagType.Gps)
                {
                    Name = "test3",
                    Longtitude = 30,
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
                new ToiModel(_guids[0])
                {
                    Description = "kludder",
                    Title = "Test Title",
                    Image =
                        "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg",
                    TagModels = new List<TagModel> {_tags[0]}
                },
                new ToiModel(_guids[1])
                {
                    Description = "kludder",
                    Title = "Test Title",
                    Image =
                        "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg",

                    TagModels = new List<TagModel> {_tags[1]}
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

            //Assert
            Assert.AreEqual(2, res.Result.Count());
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
            Assert.AreEqual(_guids[0], first?.TagModels[0].TagId);
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
            var tagsId = new List<Guid>()
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
            var toi2 = new ToiModel(_guids[0])
            {
                TagModels = new List<TagModel> {_tags[0]},
                Description = "test2",
                Title = "test2",
                Url = "test2"
            };
            var toi1 = new ToiModel(_guids[0])
            {
                Description = "test",
                Title = "test",
                Url = "test",
                TagModels = new List<TagModel> {_tags[0]}
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
            var toi2 = _tois[1];
            toi2.Id = toi1.Id;
            toi1.TagModels.Add(_tags[1]);
            toi2.TagModels.Add(_tags[2]);

            var insertStatusCode = await _dbs.InsertToiModel(toi1);

            var statusCode = await _dbs.UpdateToiModel(toi2);

            var updated = await _dbs.GetToi(_guids[0]);

            Assert.AreEqual(DatabaseStatusCode.Created, insertStatusCode);
            Assert.AreEqual(DatabaseStatusCode.Ok, statusCode);
            Assert.AreEqual(2, updated.Result.TagModels.Count);
        }
    }
}