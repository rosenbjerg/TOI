using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TOIClasses;
using TOIFeedServer.Models;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class ModelsTest
    {
        private static DatabaseService db;
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
            db = new DatabaseService(true);
        }
        [TestMethod]
        public void TagUploaded_CorrectFetch()
        {
            //Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag = new TagModel(guid, TagType.Bluetooth);

            //Act
            db.InsertTag(tag);
            var res = db.GetTagFromId(guid);
            
            //Assert
            Assert.AreEqual(typeof(TagModel), res.GetType());
        }

        [TestMethod]
        public void TagUploaded_ReturnCorrectType()
        {
            //Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag = new TagModel(guid, TagType.Bluetooth);

            //Act
            db.InsertTag(tag);
            var res = db.GetTagFromId(guid);

            //Assert
            Assert.AreEqual(TagType.Bluetooth, res.TagType);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TagBulkUploadSameIdMustReturnInvalid()
        {
            var guid1 = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag1 = new TagModel(guid1, TagType.Bluetooth);
            var tag2 = new TagModel(guid1, TagType.Bluetooth);

            var collection = new List<TagModel>()
            {
                tag1,
                tag2
            };

                db.InsertTags(collection);
            
        }

        [TestMethod]
        public void ReturnCorrectNumberTagType()
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
            db.InsertTags(collection);
            var res = db.GetTagsFromType(TagType.Bluetooth);

            //Assert
            Assert.AreEqual(2, res.Count());

        }

        [TestMethod]
        public void SaveContextModel_CorrectlySavedModel_ModelIsSaved()
        {
            // Arrange 
            var model = new ContextModel(1, "Microsoft Visual Studio", "A tour of the IDE");

            // Act
            db.InsertContext(model);
            var res = db.GetContextFromId(1);

            // Assert
            Assert.AreEqual(1, res.Id);

        }

        [TestMethod]
        public void SaveMultipleContexts_CorrectSavedModel_ModelSaved()
        {
            // Arrange
            var con1 = new ContextModel(1, "Toi title", "Toi Description");
            var con2 = new ContextModel(2, "Carlsberg tour");
            var con3 = new ContextModel(3, "Unit Test Sessions", "Where we discover the flaws of the city");

            var collection = new List<ContextModel>()
            {
                con1,
                con2,
                con3
            };

            // Act
            db.InsertContexts(collection);
            var res = db.GetAllContexts();

            // Assert
            Assert.AreEqual(3, res.Count());
        }

        [TestMethod]
        public void CreatePositionForTag_PositonAntTagOkay_Success()
        {
            // Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'),"N");
            var tag = new TagModel(guid, TagType.GPS);
            var pos = new PositionModel(tag, 40, 45);

            // Act
            db.InsertTag(tag);
            db.InsertPosition(pos);
            var res = db.GetPositionFromTagId(guid);

            // Assert
            Assert.AreEqual(40, res.X);
            Assert.AreEqual(45, res.Y);
        }

        [TestMethod]
        public void GetToiFromTagId()
        {
            // Arrange
            var guid = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var tag = new TagModel(guid, TagType.Bluetooth);
            var context = new ContextModel(2, "test");
            var toi = new ToiModel(Guid.NewGuid(), new TagInfoModel
            {
                Description = "kludder",
                Title = "Test Title",
                Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg"
            })

            {
                ContextModel = context,
                TagModel = tag
            };

            // Act
            db.InsertToi(toi);

            var result = db.GetToisByTagId(guid);
            
            // Assert
            Assert.AreEqual(1, result.Count());
        }


        [TestMethod]
        public void GetMultipleToisFromMultipleTags()
        {
            var guid1 = Guid.ParseExact("1".PadLeft(32, '0'), "N");
            var guid2 = Guid.ParseExact("2".PadLeft(32, '0'), "N");
            var tag1 = new TagModel(guid1, TagType.Bluetooth);
            var tag2 = new TagModel(guid2, TagType.Bluetooth);

            var toi1 = new ToiModel(Guid.NewGuid(), new TagInfoModel
            {
                Description = "kludder",
                Title = "Test Title",
                Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg"
            })
            {
                TagModel = tag1
            };
            var toi2 = new ToiModel(Guid.NewGuid(), new TagInfoModel
            {
                Description = "kludder",
                Title = "Test Title",
                Image = "https://scontent-amt2-1.cdninstagram.com/t51.2885-15/e35/21909339_361472870957985_3505233285414387712_n.jpg"
            })
            {
                TagModel = tag2
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


            db.InsertTags(tags);
            db.InsertToiModelList(tois);

            var result = db.GetToisByTagIds(tagsId).ToList();
            Assert.AreEqual(true, tois.All(result.Contains));
        }
    }
}
