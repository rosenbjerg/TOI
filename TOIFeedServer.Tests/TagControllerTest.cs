using System.Linq;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TOIFeedServer.Models;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class TagControllerTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            new FeedServer(false, true).Start();
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            
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
    }
}
