using ArcToCore.Net.Utils.Repository.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ArcToCore.Net.Utils.UnitTest.Client
{
    [TestClass]
    public class RestClientJsonCoreTests
    {
        private MockRepository mockRepository;
        private RestClientJsonCore CreateRestClientJsonCore() => new RestClientJsonCore();

        RestClientJsonCore restClientJsonCore;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            restClientJsonCore = this.CreateRestClientJsonCore();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Given_Json_Content_And_Must_Return_Not_Null()
        {
            //Arrange
            string content = "{'ip': '127.0.0.0'}";
            restClientJsonCore = this.CreateRestClientJsonCore();

            //Act
            var result = restClientJsonCore.RestContent(content);

            //Assert
            Assert.IsNotNull(result);
        }

    }
}