using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TOIFeedServer.Models;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class ModelsTest
    {
        private static DatabaseService  db;
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            db = new DatabaseService(true);
        }

        [ClassCleanup]
        public static void Cleanup()
        {

        }

        [TestMethod]
        public void TagUploadedCorrect()
        {
            //Arrange
            var tag = new TagModel(1);

            //Act
            db.InsertTag(tag);
            var res = db.GetTagFromID(1);
            
            //Assert
            Assert.AreEqual(typeof(TagModel), tag.GetType());
        }
    }
}
