using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TOIFeedServer.Tests
{
    [TestClass]
    public class TagControllerTest
    {
        [TestMethod]
        public void GetTagByID_ExsistingTag_ValidTagData()
        {
            Assert.Fail("Not Implemented yet");
        }

        [TestMethod]
        public void GetTagByID_NonExsistingTag_404()
        {
            Assert.IsTrue(true);
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
    }
}
