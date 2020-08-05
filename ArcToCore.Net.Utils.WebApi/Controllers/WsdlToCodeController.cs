using ArcToCore.Net.Utils.Core.Interface;
using ArcToCore.Net.Utils.Infrastructure;
using ArcToCore.Net.Utils.WebApi.Models;
using Autofac;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace ArcToCore.Net.Utils.WebApi.Controllers
{
    public class WsdlToCodeController : ApiController
    {
        [System.Web.Http.HttpPost]
        public HttpResponseMessage Post([FromBody] WsdlObject wsdlObject)
        {
            HttpContext.Current.Server.ScriptTimeout = 3000;

            #region Validate input

            if (wsdlObject == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. wsdlObject cannot be empty!");
            }

            if (wsdlObject.WsdlStream == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. WsdlStream cannot be empty!");
            }

            if (wsdlObject.WsdlStream.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad request. WsdlStream length cannot be zero!");
            }

            #endregion Validate input

            return Request.CreateResponse(HttpStatusCode.OK, GenerateCode(wsdlObject.WsdlStream, wsdlObject.NameSpace));
        }

        private IEnumerable<string> GenerateCode(byte[] streamAsByteArray, string namespaceMapping)
        {
            StringBuilder resultString = new StringBuilder
            {
                Length = 0,
                Capacity = 0
            };
            var builder = new ContainerBuilder();
            builder = Bootstrapper.ConfigureContainer();

            IConvertWsdlToCode iConvertWsdlToCode;

            using (var container = builder.Build())
            {
                iConvertWsdlToCode = container.Resolve<IConvertWsdlToCode>();
            }

            Stream stream = new MemoryStream(streamAsByteArray);

            var result = iConvertWsdlToCode.GetRequestParamsFromWsdl(stream, namespaceMapping);

            foreach (string item in result)
            {
                resultString.AppendLine(item);
            }

            return result;
        }
    }
}