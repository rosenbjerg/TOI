using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class TagControllerTest
    {
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

        }

        [TestMethod]
        public void GetTags_TagsPresent_ListOfTags()
        {

        }

        [TestMethod]
        public void GetTags_NoTags_EmptyList()
        {

        }
    }
}
