using ArcToCore.Net.Utils.Core.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ArcToCore.Net.Utils.Core.Interface
{
    public interface IObjectHandler
    {
        string GetFreindlyTypeName(LanguageEnum lan, JProperty value);

        void WalkNode(JToken node, Action<JObject> action);

        Assembly GetServiceInspector(Stream stream);

        List<string> GenerateDtoAsString(Stream source, string nameSpace);

        List<string> ReturnOperationsParameters(string source);
    }
}