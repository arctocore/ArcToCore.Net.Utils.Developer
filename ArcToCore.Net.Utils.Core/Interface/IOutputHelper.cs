using ArcToCore.Net.Utils.Core.Enums;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ArcToCore.Net.Utils.Core.Interface
{
    public interface IOutputHelper
    {
        string FirstCharToUpper(string input);

        string GenerateForLanguage(LanguageEnum lan, JProperty type, LanAttributes props);

        string SplitString(string str);

        string GenerateCode(bool extractfiles, Dictionary<string, List<string>> checkClassNamesDic, string pathTemp, LanguageEnum lan, string namespaceMapping);

        List<string> GenerateCodeList(bool extractfiles, Dictionary<string, List<string>> checkClassNamesDic, string path, LanguageEnum lan, LanAttributes prop, string namespaceMapping);
    }
}