using ArcToCore.Net.Utils.Core.Interface;
using System.Collections.Generic;
using System.IO;

namespace ArcToCore.Net.Utils.Repository.Converter
{
    public class ConvertWsdlToCode : IConvertWsdlToCode
    {
        #region Interfaces

        private readonly IObjectHandler _iObjectHandler;

        #endregion Interfaces

        public ConvertWsdlToCode(IObjectHandler iObjectHandler)
        {
            this._iObjectHandler = iObjectHandler;
        }

        public List<string> GetRequestParamsFromWsdl(Stream source, string namespaceMapping)
        {
            var result = _iObjectHandler.GenerateDtoAsString(source, namespaceMapping);

            return result;
        }
    }
}