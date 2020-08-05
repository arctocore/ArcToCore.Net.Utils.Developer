using System.Diagnostics;
using System.Threading.Tasks;

namespace ArcToCore.Core.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            string jsonUrl = "http://ergast.com/api/f1/2004/1/results.json";
            string jsonToCodeApiUrl = "http://localhost:11728/JsonToCode";
            string path = @"c:\tmp\code\";
            string nameSpace = "ArcToCore.JsonCode";
            var msg = await JsonToCode.DownloadRestToCodeAsync(jsonUrl, jsonToCodeApiUrl, nameSpace, path);
            Debug.Write(msg);
        }
    }
}