using ArcToCore.Net.Utils.Core.Interface;
using ArcToCore.Net.Utils.Infrastructure;
using ArcToCore.Net.Utils.WebApi.Models;
using Autofac;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ArcToCore.Net.Utils.WebApi.Controllers
{
    public class JsonToCodeController : ApiController
    {
        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post([FromBody] JsonObject jsonObject)
        {
            #region Validate input

            if (jsonObject == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. JsonObject cannot be empty!");
            }

            if (string.IsNullOrEmpty(jsonObject.Culture))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. Culture cannot be empty!");
            }

            if (jsonObject.JsonBytes == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. JsonBytes cannot be empty!");
            }

            if (jsonObject.JsonBytes.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. JsonBytes length cannot be zero!");
            }

            #endregion Validate input

            char[] chars = new char[jsonObject.JsonBytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(jsonObject.JsonBytes, 0, chars, 0, jsonObject.JsonBytes.Length);
            string str = new string(chars);

            string jsonString = str;
            HttpContext.Current.Server.ScriptTimeout = 3000;
            return Request.CreateResponse(HttpStatusCode.OK, GenerateCode(jsonString, jsonObject.Culture, jsonObject.NameSpace));
        }

        public IEnumerable<string> GenerateCode(string jsonString, string culture, string namespaceMapping)
        {
            var builder = new ContainerBuilder();
            builder = Bootstrapper.ConfigureContainer();

            IRestClientJsonCore iRestClientJsonCore;
            IConvertJsonToPoco iConvertJsonToPoco;

            using (var container = builder.Build())
            {
                iRestClientJsonCore = container.Resolve<IRestClientJsonCore>();
                iConvertJsonToPoco = container.Resolve<IConvertJsonToPoco>();
            }

            string path = string.Empty;

            path = string.Empty;
            var result = iConvertJsonToPoco.CodeGenerate(jsonString, culture, namespaceMapping);
            return result;
        }
    }
}