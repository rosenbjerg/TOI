using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TOIClasses;
using TOIFeedServer.Models;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class ModelsTest
    {
        private static DatabaseService _db;
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
            _db = new DatabaseService(true);
        }
        [TestMethod]
        public async Task TagUploaded_CorrectFetch()
        {
            //Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag = new TagModel(guid, TagType.Bluetooth);

            //Act
            await _db.InsertTag(tag);
            var res = await _db.GetTagFromId(guid);
            
            //Assert
            Assert.AreEqual(typeof(TagModel), res.Result.GetType());
        }

        [TestMethod]
        public async Task TagUploaded_ReturnCorrectType()
        {
            //Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag = new TagModel(guid, TagType.Bluetooth);

            //Act
            await _db.InsertTag(tag);
            var res = await _db.GetTagFromId(guid);

            //Assert
            Assert.AreEqual(TagType.Bluetooth, res.Result.TagType);
        }

        [TestMethod]
        public async Task ReturnCorrectNumberTagType()
        {
            //Arrange
            var guid1 = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var guid2 = Guid.ParseExact("2".PadLeft(32, '0'), "N");
            var guid3 = Guid.ParseExact("3".PadLeft(32, '0'), "N");

            var tag1 = new TagModel(guid1, TagType.Bluetooth);
            var tag2 = new TagModel(guid2, TagType.Bluetooth);
            var tag3 = new TagModel(guid3, TagType.GPS);

            var collection = new List<TagModel>()
            {
                tag1,
                tag2,
                tag3
            };

            //Act
            await _db.InsertTags(collection);
            var res = _db.GetTagsFromType(TagType.Bluetooth);

            //Assert
            Assert.AreEqual(2, res.Result.Count());

        }

        [TestMethod]
        public async Task SaveContextModel_CorrectlySavedModel_ModelIsSaved()
        {
            // Arrange 
            var guid = Guid.ParseExact("1".PadLeft(32,'0'), "N");
            var model = new ContextModel(guid, "Microsoft Visual Studio", "A tour of the IDE");

            // Act
            await _db.InsertContext(model);
            var res = await _db.GetContextFromId(guid);

            // Assert
            Assert.AreEqual(guid, res.Result.Id);

        }

        [TestMethod]
        public async Task SaveMultipleContexts_CorrectSavedModel_ModelSaved()
        {
            // Arrange

            var guid1 = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var guid2 = Guid.ParseExact("2".PadLeft(32, '0'), "N");
            var guid3 = Guid.ParseExact("3".PadLeft(32, '0'), "N");

            var con1 = new ContextModel(guid1, "Toi title", "Toi Description");
            var con2 = new ContextModel(guid2, "Carlsberg tour");
            var con3 = new ContextModel(guid3, "Unit Test Sessions", "Where we discover the flaws of the city");

            var collection = new List<ContextModel>()
            {
                con1,
                con2,
                con3
            };

            // Act
            await _db.InsertContexts(collection);
            var res = await _db.GetAllContexts();

            // Assert
            Assert.AreEqual(3, res.Result.Count());
        }

        [TestMethod]
        public async Task CreatePositionForTag_PositonAndTagOkay_Success()
        {
            // Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'),"N");
            var tag = new TagModel(guid, TagType.GPS);
            var pos = new PositionModel(tag, 40, 45);

            // Act
            await _db.InsertTag(tag);
            await _db.InsertPosition(pos);
            var res = await _db.GetPositionFromTagId(guid);

            // Assert
            Assert.AreEqual(DatabaseStatusCode.Ok, res.Status);
            Assert.AreEqual(40, res.Result.X);
            Assert.AreEqual(45, res.Result.Y);
        }

        [TestMethod]
        public async Task GetToiFromTagId()
        {
            // Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag = new List<TagModel>{ new TagModel(guid, TagType.Bluetooth)};
            var context = new ContextModel(guid, "test");
            var toi = new ToiModel(Guid.NewGuid(), new TagInfoModel
            {
                Description = "kludder",
                Title = "Test Title",
                Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg"
            })

            {
                ContextModel = context,
                TagModels = tag
            };

            // Act
            var test = await _db.InsertToiModel(toi);

            var result = await _db.GetToisByTagIds(new [] {guid});

            // Assert
            Assert.AreEqual(DatabaseStatusCode.Ok, result.Status);
            Assert.AreEqual(DatabaseStatusCode.Created, test);
            Assert.AreEqual(1, result.Result.Count());
            Assert.AreEqual(guid, result.Result.FirstOrDefault().TagModels[0].TagId);
        }

        [TestMethod]
        public async Task InsertToiModelListReturnDataBaseStatusCodeListContainsDuplicates()
        {
            var guid1 = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag1 = new TagModel(guid1, TagType.Bluetooth);
            var toi1 = new ToiModel(Guid.NewGuid(),
                new TagInfoModel
                {
                    Description = "kludder",
                    Title = "Test Title",
                    Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg",
                })
            {
                TagModels = new List<TagModel> { tag1 }
            };
            List<ToiModel> toiModels = new List<ToiModel>()
            {
                toi1,
                toi1
            };
            var result = await _db.InsertToiModelList(toiModels);

            Assert.AreEqual(DatabaseStatusCode.ListContainsDuplicate, result);
        }

        [TestMethod]
        public async Task GetMultipleToisFromMultipleTags()
        {
            var guid1 = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var guid2 = Guid.ParseExact("2".PadLeft(32, '0'), "N");
            var tag1 = new TagModel(guid1, TagType.Bluetooth);
            var tag2 = new TagModel(guid2, TagType.Bluetooth);

            var toi1 = new ToiModel(Guid.NewGuid(), 
            new TagInfoModel
            {
                Description = "kludder",
                Title = "Test Title",
                Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg",
            })
            {
                TagModels = new List<TagModel> { tag1 }
            };
            var toi2 = new ToiModel(Guid.NewGuid(), new TagInfoModel
            {
                Description = "kludder",
                Title = "Test Title",
                Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg"
            })
            {
                TagModels = new List<TagModel> { tag2 }
            };
            var tags = new List<TagModel>()
            {
                tag1,
                tag2
            };
            var tois = new List<ToiModel>()
            {
                toi1,
                toi2
            };
            var tagsId= new List<Guid>()
            {
                tag1.TagId,
                tag2.TagId
            };


            await _db.InsertTags(tags);
            await _db.InsertToiModelList(tois);

            var result = await _db.GetToisByTagIds(tagsId);

            Assert.AreEqual(true, tois.All(result.Result.Contains));
        }
    }
}
