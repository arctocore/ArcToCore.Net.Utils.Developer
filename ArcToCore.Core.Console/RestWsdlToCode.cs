using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ArcToCore.Core.Console
{
    public class RestWsdlToCode
    {
        public static async Task<string> DownloadRestWsdlToCodeAsync(
          string wsdlUrl,
          string wsdlToCodeApiUrl,
          string nameSpace,
          string downloadPath)
        {
            string str1 = "OK ";
            try
            {
                if (string.IsNullOrEmpty(wsdlUrl))
                    return "wsdlUrl is empty ";
                if (string.IsNullOrEmpty(wsdlToCodeApiUrl))
                    return "wsdlToCodeApiUrl is empty ";
                if (string.IsNullOrEmpty(downloadPath))
                    return "No download path present ";
                string path = downloadPath;
                Uri uri = new Uri(wsdlUrl);
                if (uri.Query == string.Empty)
                    uri = new UriBuilder(uri) { Query = "WSDL" }.Uri;
                byte[] array;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    WebRequest.Create(uri).GetResponse().GetResponseStream().CopyTo((Stream)memoryStream);
                    array = memoryStream.ToArray();
                }
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject((object)new RestWsdlToCode.WsdlObject()
                {
                    WsdlStream = array,
                    NameSpace = nameSpace
                }), Encoding.UTF8, "application/json");
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage httpResponseMessage = await client.PostAsync(wsdlToCodeApiUrl, (HttpContent)stringContent))
                    {
                        using (HttpContent content = httpResponseMessage.Content)
                        {
                            str1 = HttpUtility.HtmlDecode(content.ReadAsStringAsync().Result);
                            if (str1 == null)
                                return "Code Generation failed ";
                            str1 = str1.Remove(0, 2);
                            str1 = str1.Remove(str1.Length - 2);
                            List<char> list1 = str1.ToList<char>();
                            StringBuilder stringBuilder = new StringBuilder();
                            char ch;
                            for (int index = 0; index < list1.Count; ++index)
                            {
                                ch = list1[index];
                                if (ch.Equals(','))
                                {
                                    ch = list1[index - 1];
                                    if (ch.Equals('"'))
                                        list1.Remove(list1[index - 1]);
                                    stringBuilder.Append(list1[index - 1]);
                                }
                                else
                                    stringBuilder.Append(list1[index]);
                            }
                            str1 = stringBuilder.ToString();
                            str1 = Regex.Replace(str1, "\\\\n", "");
                            str1 = Regex.Replace(str1, "\\\\r", "");
                            str1 = Regex.Replace(str1, "\\\\", "");
                            str1 = Regex.Replace(str1, "\"\\\"", "");
                            str1 = Regex.Replace(str1, "&lt;", "<");
                            str1 = Regex.Replace(str1, "&gt;", ">");
                            List<string> list2 = ((IEnumerable<string>)str1.Split(',')).ToList<string>();
                            for (int index1 = 0; index1 < list2.Count; ++index1)
                            {
                                string str2 = list2[index1];
                                ch = str2[str2.Length - 1];
                                if (ch.Equals('"'))
                                    str2 = str2.Remove(str2.Length - 1);
                                string[] strArray = str2.Split(' ');
                                for (int index2 = 0; index2 < strArray.Length; ++index2)
                                {
                                    if (strArray[index2].Contains("class"))
                                    {
                                        string str3 = strArray[index2 + 1];
                                        if (index2 <= strArray.Length)
                                        {
                                            using (FileStream fileStream = System.IO.File.Create(path + str3 + ".cs"))
                                            {
                                                byte[] bytes = new UTF8Encoding(true).GetBytes(((SyntaxNode)SyntaxNodeExtensions.NormalizeWhitespace<SyntaxNode>(CSharpSyntaxTree.ParseText(str2, (CSharpParseOptions)null, "", (Encoding)null, new CancellationToken()).GetRoot(new CancellationToken()), "    ", "\r\n", false)).ToFullString());
                                                fileStream.Write(bytes, 0, bytes.Length);
                                            }
                                        }
                                        else
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                return str1 + " ";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public class WsdlObject
        {
            public byte[] WsdlStream { get; set; }

            public string NameSpace { get; set; }
        }
    }
}