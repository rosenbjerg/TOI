using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var tag = new TagModel(1, TagType.Bluetooth);

            //Act
            db.InsertTag(tag);
            var res = db.GetTagFromID(1);
            
            //Assert
            Assert.AreEqual(typeof(TagModel), res.GetType());
        }

        [TestMethod]
        public void TagUploaded_ReturnCorrectType()
        {
            //Arrange
            var tag = new TagModel(1, TagType.Bluetooth);

            //Act
            db.InsertTag(tag);
            var res = db.GetTagFromID(1);

            //Assert
            Assert.AreEqual(TagType.Bluetooth, res.TagType);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TagBulkUploadSameIdMustReturnInvalid()
        {
            var tag1 = new TagModel(1, TagType.Bluetooth);
            var tag2 = new TagModel(1, TagType.Bluetooth);

            var collection = new List<TagModel>()
            {
                tag1,
                tag2
            };

                db.InsertTags(collection);
                Assert.IsTrue(true);
            
        }

        [TestMethod]
        public void ReturnCorrectNumberTagType()
        {
            //Arrange
            var tag1 = new TagModel(1, TagType.Bluetooth);
            var tag2 = new TagModel(2, TagType.Bluetooth);
            var tag3 = new TagModel(3, TagType.GPS);

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
            var tag = new TagModel(1, TagType.GPS);
            var pos = new PositionModel(tag, 40, 45);

            // Act
            db.InsertTag(tag);
            db.InsertPosition(pos);
            var res = db.GetPositionFromTagId(1);

            // Assert
            Assert.AreEqual(40, res.X);
            Assert.AreEqual(45, res.Y);
        }

        [TestMethod]
        public void GetToiFromTagId()
        {
            // Arrange
            var tag = new TagModel(3, TagType.Bluetooth);
            var context = new ContextModel(2, "test");
            var toi = new ToiModel(1, "test")
            {
                ContextModel = context,
                TagModel = tag
            };

            // Act
            db.InsertToi(toi);
            var result = db.GetToisByTagId(3);
            
            // Assert
            Assert.AreEqual(1, result.Count());
        }
    }
}
