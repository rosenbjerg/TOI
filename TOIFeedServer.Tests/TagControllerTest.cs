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
            
        }

        [TestMethod]
        public void GetTagByID_ExsistingTag_ValidTagData()
        {
            
        }

        [TestMethod]
        public void GetTagByID_NonExsistingTag_404()
        {
            
        }

        [TestMethod]
        public void UploadTagInfo_ValidInfoValidTag_200()
        {
            
        }

        [TestMethod]
        public void UploadTagInfo_InvalidTagID_404()
        {
            
        }

        [TestMethod]
        public void UploadTagInfo_ValidTagInvalidData_400()
        {

        }

        [TestMethod]
        public void GetTags_TagsPresent_ListOfTags()
        {

        }

        [TestMethod]
        public void GetTags_NoTags_EmptyList()
        {

        }

        [TestMethod]
        public void GetHelloWorld_NoInput_Valid()
        {
            //Arrange
            var page = "http://localhost:5000/hello";
            var client = new HttpClient();
            
            //Act
            var task = client.GetStringAsync(page);
            task.Wait();
            
            //Assert
            Assert.AreEqual("Hello World", task.Result);
        }
    }
}
