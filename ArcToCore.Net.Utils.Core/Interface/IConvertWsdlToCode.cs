using System.Collections.Generic;
using System.IO;

namespace ArcToCore.Net.Utils.Core.Interface
{
    public interface IConvertWsdlToCode
    {
        List<string> GetRequestParamsFromWsdl(Stream source, string namespaceMapping);
    }
}