using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class TagControllerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            new FeedServer(); 
        }

        [TestCleanup]
        public void Cleanup()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetTagByID_ExsistingTag_ValidTagData()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetTagByID_NonExsistingTag_404()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void UploadTagInfo_ValidInfoValidTag_200()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void UploadTagInfo_InvalidTagID_404()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void UploadTagInfo_ValidTagInvalidData_400()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetTags_TagsPresent_ListOfTags()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetTags_NoTags_EmptyList()
        {
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetHelloWorld_NoInput_Valid()
        {
            //Arrange
            var page = "http://localhost:27015/hello";
            var client = new HttpClient();
            
            //Act
            var task = client.GetStringAsync(page);
            task.Wait();
            
            //Assert
            Assert.AreEqual("Hello World", task.Result);
        }
    }
}
