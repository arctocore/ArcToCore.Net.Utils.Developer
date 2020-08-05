using ArcToCore.Net.Utils.Core.Enums;
using ArcToCore.Net.Utils.Core.Interface;
using ArcToCore.Net.Utils.Repository.Output;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ArcToCore.Net.Utils.UnitTest.Output
{
    [TestClass]
    public class OutputHelperTests
    {
        private MockRepository mockRepository;

        private Mock<IObjectHandler> mockObjectHandler;

        [TestInitialize]
        public void TestInitialize()
        {
            //Arrange
            mockRepository = new MockRepository(MockBehavior.Loose);
            mockObjectHandler = this.mockRepository.Create<IObjectHandler>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public void Given_The_First_Char_Is_Lowercase()
        {
            //Arrange
            OutputHelper outputHelper = this.CreateOutputHelper();

            //Act
            var result = outputHelper.FirstCharToUpper("s");

            //Assert
            Assert.AreEqual("S", result);
        }

        [TestMethod]
        public void Given_The_String_Contains_Brackets()
        {
            //Arrange
            OutputHelper outputHelper = this.CreateOutputHelper();

            //Act
            var result = outputHelper.SplitString("[0].Object");

            //Assert
            Assert.AreEqual("Object", result);
        }

        [TestMethod]
        public void Given_Not_Extracting_GeneratedCode_And_Return_Public_Class_Declaration()
        {
            //Arrange
            OutputHelper outputHelper = this.CreateOutputHelper();

            //Act
            bool extractFiles = false;

            string propString = "private string millisObj; public void setMillis(string millisObj){this.millisObj = millisObj;} public string getMillis(){return this.millisObj;}";

            var lst = new List<string>() { propString };
            Dictionary<string, List<string>> jTokenDic = new Dictionary<string, List<string>>()
            {
              {"ClassObj", lst }
            };

            string path = @"c:\tmp";
            var result = outputHelper.GenerateCode(extractFiles, jTokenDic, path, LanguageEnum.CSharp, "Example.Ex");

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Given_Not_Extracting_GeneratedCode_And_Return_Public_Class_Declaration_As_ListOfStrings()
        {
            //Arrange
            OutputHelper outputHelper = this.CreateOutputHelper();

            //Act
            bool extractFiles = false;

            string propString = "private string millisObj; public void setMillis(string millisObj){this.millisObj = millisObj;} public string getMillis(){return this.millisObj;}";

            var lst = new List<string>() { propString };
            Dictionary<string, List<string>> jTokenDic = new Dictionary<string, List<string>>()
            {
                {"ClassObj", lst }
            };
            LanAttributes props = LanAttributes.None;
            LanguageEnum lan = LanguageEnum.CSharp;
            string path = @"c:\tmp";
            string namespaceMapping = "namespace";
            var result = outputHelper.GenerateCodeList(extractFiles, jTokenDic, path, lan, props, namespaceMapping);

            //Assert
            Assert.IsNotNull(result);
        }

        private OutputHelper CreateOutputHelper() => new OutputHelper(this.mockObjectHandler.Object);

        [TestMethod]
        public void Given_A_JProperty_Must_Return_CSharpProperties()
        {
            //Arrange
            OutputHelper outputHelper = this.CreateOutputHelper();
            JObject jsonObject = new JObject(new JProperty("Object", "10"));
            LanAttributes props = LanAttributes.None;
            //Act
            var result = outputHelper.GenerateForLanguage(Core.Enums.LanguageEnum.CSharp, jsonObject.Property("Object"), props);

            //Assert
            Assert.IsNotNull(result);
        }
    }
}