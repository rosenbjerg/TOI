using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class TagControllerTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            new FeedServer(); 
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            
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
            var page = "http://127.0.0.1:7474/hello";
            var client = new HttpClient();
            
            //Act
            var task = client.GetStringAsync(page);
            task.Wait();
            
            //Assert
            Assert.AreEqual("Hello World", task.Result);
        }

        [TestMethod]
        public void Database_Insert_Toi()
        {
            var service = new DatabaseService(true);
            var model = new ToiModel("test");

            service.InsertToiModel(model);

            Assert.AreEqual("test", service.GetToiModelFromContext("test"));
        }
    }
}
