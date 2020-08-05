using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ArcToCore.Core.Console
{
    public class JsonToCode
    {
        public static async Task<string> DownloadRestToCodeAsync(
          string jsonUrl,
          string jsonToCodeApiUrl,
          string nameSpace,
          string downloadPath)
        {
            string str1 = "OK ";
            try
            {
                if (string.IsNullOrEmpty(jsonUrl))
                    return "jsonUrl is empty ";
                if (string.IsNullOrEmpty(jsonToCodeApiUrl))
                    return "jsonToCodeApiUrl is empty ";
                if (string.IsNullOrEmpty(downloadPath))
                    return "No download path present ";
                string path = downloadPath;
                string str2 = string.Empty;
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(jsonUrl);
                    Task<HttpResponseMessage> async = httpClient.GetAsync(jsonUrl);
                    if (async != null)
                        str2 = async.Result.Content.ReadAsStringAsync().Result;
                }
                byte[] numArray = new byte[str2.Length * 2];
                Buffer.BlockCopy((Array)str2.ToCharArray(), 0, (Array)numArray, 0, numArray.Length);
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject((object)new JsonToCode.JsonObject()
                {
                    Culture = "en-us",
                    JsonBytes = numArray,
                    NameSpace = nameSpace
                }), Encoding.UTF8, "application/json");
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage httpResponseMessage = await client.PostAsync(jsonToCodeApiUrl, (HttpContent)stringContent))
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
                            str1 = Regex.Replace(str1, "\\\\t", "");
                            str1 = Regex.Replace(str1, "\\\\", "");
                            str1 = Regex.Replace(str1, "\"\\\"", "");
                            str1 = Regex.Replace(str1, "&lt;", "<");
                            str1 = Regex.Replace(str1, "&gt;", ">");
                            List<string> list2 = ((IEnumerable<string>)str1.Split(',')).ToList<string>();
                            for (int index1 = 0; index1 < list2.Count; ++index1)
                            {
                                string text = list2[index1];
                                ch = text[text.Length - 1];
                                if (ch.Equals('"'))
                                    text = text.Remove(text.Length - 1);
                                string[] strArray = text.Split(' ');
                                for (int index2 = 0; index2 < strArray.Length; ++index2)
                                {
                                    if (strArray[index2].Contains("class"))
                                    {
                                        string str3 = strArray[index2 + 1].Split('{').GetValue(0).ToString();
                                        if (index2 <= strArray.Length)
                                        {
                                            using (FileStream fileStream = File.Create(path + str3 + ".cs"))
                                            {
                                                byte[] bytes = new UTF8Encoding(true).GetBytes(((SyntaxNode)SyntaxNodeExtensions.NormalizeWhitespace<SyntaxNode>(CSharpSyntaxTree.ParseText(text, (CSharpParseOptions)null, "", (Encoding)null, new CancellationToken()).GetRoot(new CancellationToken()), "    ", "\r\n", false)).ToFullString());
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

        public class JsonObject
        {
            public byte[] JsonBytes { get; set; }

            public string Culture { get; set; }

            public string NameSpace { get; set; }
        }
    }
}