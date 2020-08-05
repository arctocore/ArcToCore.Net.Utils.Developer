using ArcToCore.Net.Utils.Core.Enums;
using ArcToCore.Net.Utils.Core.Interface;
using Ganss.XSS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcToCore.Net.Utils.Repository.Output
{
    public class OutputHelper : IOutputHelper
    {
        private IObjectHandler _parser;

        public OutputHelper(IObjectHandler parser)
        {
            _parser = parser;
        }

        #region public

        public string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("Input is empty!");
            return input.First().ToString().ToUpper() + String.Join("", input.Skip(1));
        }

        public string FirstCharToLower(string input)
        {
            if (String.IsNullOrEmpty(input))
                throw new ArgumentException("Input is empty!");
            return input.First().ToString().ToLower() + String.Join("", input.Skip(1));
        }

        public string GenerateForLanguage(LanguageEnum lan, JProperty type, LanAttributes props)
        {
            string _lan = string.Empty;
            _lan = CodeDto(lan, type, props);
            return _lan;
        }

        public string SplitString(string str)
        {
            Regex rgx = new Regex("[^a-zA-Z]");
            return rgx.Replace(str, "");
        }

        public string GenerateCode(bool extractfiles, Dictionary<string, List<string>> checkClassNamesDic, string path, LanguageEnum lan, string namespaceMapping)
        {
            StringBuilder codeBuilder = new StringBuilder();

            //goes for lan c# and java

            string lanClassDef = "  public class";
            string lanbracesStart = "  {";
            string lanBrracesEnd = "  }";

            if (lan == LanguageEnum.PHP)
            {
                lanClassDef = "  class";
                lanbracesStart = "  {";
                lanBrracesEnd = "  }";
            }

            foreach (KeyValuePair<string, List<string>> codeClassItem in checkClassNamesDic)
            {
                switch (lan)
                {
                    case LanguageEnum.CSharp:
                        //using for C# class
                        if (!string.IsNullOrEmpty(namespaceMapping))
                        {
                            codeBuilder.AppendLine("namespace " + namespaceMapping + "\n{");
                        }
                        break;

                    case LanguageEnum.PHP:
                        break;

                    case LanguageEnum.Java:
                        //using for jAVA class
                        if (!string.IsNullOrEmpty(namespaceMapping))
                        {
                            codeBuilder.AppendLine("package " + namespaceMapping + ";");
                        }
                        break;
                }
                codeBuilder.AppendLine(lanClassDef + " " + codeClassItem.Key);
                codeBuilder.AppendLine(lanbracesStart);
                codeClassItem.Value.ForEach(delegate (string props)
                {
                    codeBuilder.AppendLine("  " + props);
                });
                codeBuilder.AppendLine(lanBrracesEnd);
                switch (lan)
                {
                    case LanguageEnum.CSharp:
                        //using for C# class
                        if (!string.IsNullOrEmpty(namespaceMapping))
                        {
                            codeBuilder.AppendLine("\n}");
                        }
                        break;
                }
                if (extractfiles)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    using (TextWriter writer = File.CreateText(path + @"\" + codeClassItem.Key))
                    {
                        writer.Write(codeBuilder);

                        codeBuilder.Length = 0;
                    }
                }
            }

            return codeBuilder.ToString();
        }

        public List<string> GenerateCodeList(bool extractfiles,
            Dictionary<string, List<string>> checkClassNamesDic,
            string path,
            LanguageEnum lan,
            LanAttributes prop,
            string namespaceMapping)
        {
            StringBuilder codeBuilder = new StringBuilder();
            List<string> codeList = new List<string>();
            //goes for lan c# and java

            StringBuilder cSharpUsings = new StringBuilder()
            {
                Capacity = 0,
                Length = 0
            };

            cSharpUsings.AppendLine("using System;");
            cSharpUsings.AppendLine("using System.Collections.Generic;");

            StringBuilder javaUsings = new StringBuilder()
            {
                Capacity = 0,
                Length = 0
            };

            javaUsings.AppendLine("import java.util.Date;");
            javaUsings.AppendLine("import java.util.ArrayList;");

            ///Meaning that we have chosen Datacontracts and DataMembers

            switch (prop)
            {
                case LanAttributes.None:
                    break;

                case LanAttributes.DataContract:
                    cSharpUsings.AppendLine("using System.Runtime.Serialization;");
                    break;

                case LanAttributes.Json:
                    cSharpUsings.AppendLine("using Newtonsoft.Json;");
                    break;
            }

            string lanClassDef = string.Empty;
            string lanbracesStart = string.Empty;
            string lanBrracesEnd = string.Empty;

            switch (lan)
            {
                case LanguageEnum.CSharp:
                    //using for C# class
                    lanClassDef = "  public class";
                    lanbracesStart = "  {";
                    lanBrracesEnd = "  }";

                    break;

                case LanguageEnum.PHP:
                    //using for PHP class
                    lanClassDef = "  class";
                    lanbracesStart = "  {";
                    lanBrracesEnd = "  }";
                    break;

                case LanguageEnum.Java:
                    //using for jAVA class
                    lanClassDef = "  public class";
                    lanbracesStart = "  {";
                    lanBrracesEnd = "  }";
                    break;
            }

            foreach (KeyValuePair<string, List<string>> codeClassItem in checkClassNamesDic)
            {
                switch (lan)
                {
                    case LanguageEnum.CSharp:
                        //using for C# class
                        codeBuilder.AppendLine(cSharpUsings.ToString());

                        break;

                    case LanguageEnum.PHP:
                        break;

                    case LanguageEnum.Java:
                        //using for jAVA class
                        codeBuilder.AppendLine(javaUsings.ToString());
                        break;
                }

                switch (lan)
                {
                    case LanguageEnum.CSharp:
                        //using for C# class
                        if (!string.IsNullOrEmpty(namespaceMapping))
                        {
                            codeBuilder.AppendLine("namespace " + namespaceMapping + "\n{");
                        }

                        break;

                    case LanguageEnum.PHP:
                        break;

                    case LanguageEnum.Java:
                        if (!string.IsNullOrEmpty(namespaceMapping))
                        {
                            codeBuilder.AppendLine("package " + namespaceMapping + ";");
                        }
                        break;
                }

                switch (prop)
                {
                    case LanAttributes.None:
                        break;

                    case LanAttributes.DataContract:
                        codeBuilder.AppendLine("  [DataContract]");
                        break;

                    case LanAttributes.Json:
                        break;
                }
                codeBuilder.AppendLine(lanClassDef + " " + codeClassItem.Key);
                codeBuilder.AppendLine(lanbracesStart);
                codeClassItem.Value.ForEach(delegate (string props)
                {
                    codeBuilder.AppendLine("  " + props);
                });
                codeBuilder.AppendLine(lanBrracesEnd);

                switch (lan)
                {
                    case LanguageEnum.CSharp:
                        //using for C# class
                        if (!string.IsNullOrEmpty(namespaceMapping))
                        {
                            codeBuilder.AppendLine("\n}");
                        }

                        break;
                }

                var sanitizer = new HtmlSanitizer();
                var sanitized = sanitizer.Sanitize(codeBuilder.ToString());
                codeList.Add(sanitized);

                //we need the class to get it from angularjs in the filter and represent it in the accordian

                codeBuilder.Length = 0;
                codeBuilder.Capacity = 0;

                if (extractfiles)
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    using (TextWriter writer = File.CreateText(path + @"\" + codeClassItem.Key))
                    {
                        writer.Write(codeBuilder);

                        codeBuilder.Length = 0;
                    }
                }
            }

            return codeList;
        }

        #endregion public

        #region private

        private string CodeDto(LanguageEnum lan, JProperty jsonObj, LanAttributes props)
        {
            string prop = ExtractLanProperties(lan, jsonObj);
            return BuildFor_Code_Props(prop, jsonObj, props, lan);
        }

        private string ExtractLanProperties(LanguageEnum lan, JProperty jsonObj)
        {
            var _type = _parser.GetFreindlyTypeName(lan, jsonObj);

            string prop = string.Empty;
            if (jsonObj.Value.Type == JTokenType.Array)
            {
                var propChildren = jsonObj.Children().ToList();
                foreach (JToken jTokenItem in propChildren)
                {
                    string arrayType = string.Empty;
                    switch (lan)
                    {
                        case LanguageEnum.CSharp:
                            arrayType = "List";
                            break;

                        case LanguageEnum.PHP:
                            arrayType = "array";
                            break;

                        case LanguageEnum.Java:
                            arrayType = "ArrayList";
                            break;
                    }

                    if (lan != LanguageEnum.PHP)
                    {
                        if (jTokenItem.Any(a => a.Type == JTokenType.String))
                        {
                            prop = arrayType + "&lt;string&gt;";
                            break;
                        }
                        else
                        {
                            prop = arrayType + "&lt;" + FirstCharToUpper(jsonObj.Name) + "&gt;";
                            break;
                        }
                    }
                    else if (lan == LanguageEnum.PHP)
                    {
                        prop = arrayType;
                        break;
                    }
                }
            }
            else if (jsonObj.Value.Type == JTokenType.Object)
            {
                prop = FirstCharToUpper(jsonObj.Name);
            }
            else
            {
                prop = _type;
            }

            return prop;
        }

        // use this for both language
        private string BuildFor_Code_Props(string prop, JProperty jsonObj, LanAttributes props, LanguageEnum lan)
        {
            StringBuilder setgetBuilder = new StringBuilder
            {
                Length = 0,
                Capacity = 0
            };

            string instance = FirstCharToLower(jsonObj.Name);
            string classObj = FirstCharToUpper(jsonObj.Name);
            string quate = @"""";

            if (lan != LanguageEnum.PHP)
            {
                switch (props)
                {
                    case LanAttributes.None:
                        setgetBuilder.AppendLine("  private " + prop + " " + instance + ";");
                        break;

                    case LanAttributes.DataContract:
                        setgetBuilder.AppendLine("  [DataMember(Name = " + quate + instance + quate + ")]");
                        setgetBuilder.AppendLine("    private " + prop + " " + instance + ";");
                        break;

                    case LanAttributes.Json:
                        setgetBuilder.AppendLine("  [JsonProperty(" + quate + instance + quate + ")]");
                        setgetBuilder.AppendLine("    private " + prop + " " + instance + ";");
                        break;
                }
            }
            else if (lan == LanguageEnum.PHP)
            {
                if (prop.Equals("array"))
                {
                    setgetBuilder.AppendLine("  private $" + instance + "s; //" + prop + "(" + instance + ")");
                }
                else
                {
                    setgetBuilder.AppendLine("  private $" + instance + "; //" + prop);
                }
            }

            if (lan != LanguageEnum.PHP)
            {
                //remember type array php
                if (jsonObj.Name.Equals("long"))
                {
                    setgetBuilder.AppendLine("    public void Set" + classObj + "(" + prop + " " + "@long" + ")");
                }
                else if (jsonObj.Name.Equals("int"))
                {
                    setgetBuilder.AppendLine("    public void Set" + classObj + "(" + prop + " " + "@int" + ")");
                }
                else if (jsonObj.Name.Equals("short"))
                {
                    setgetBuilder.AppendLine("    public void Set" + classObj + "(" + prop + " " + "@short" + ")");
                }
                else if (jsonObj.Name.Equals("float"))
                {
                    setgetBuilder.AppendLine("    public void Set" + classObj + "(" + prop + " " + "@float" + ")");
                }
                else if (jsonObj.Name.Equals("double"))
                {
                    setgetBuilder.AppendLine("    public void Set" + classObj + "(" + prop + " " + "@double" + ")");
                }
                else
                {
                    setgetBuilder.AppendLine("    public void Set" + classObj + "(" + prop + " " + FirstCharToLower(jsonObj.Name) + ")");
                }

                setgetBuilder.AppendLine("    {");

                if (jsonObj.Name.Equals("long"))
                {
                    setgetBuilder.AppendLine("        this." + instance + "=" + "@long" + ";");
                }
                else if (jsonObj.Name.Equals("int"))
                {
                    setgetBuilder.AppendLine("        this." + instance + "=" + "@int" + ";");
                }
                else if (jsonObj.Name.Equals("short"))
                {
                    setgetBuilder.AppendLine("        this." + instance + "=" + "@short" + ";");
                }
                else if (jsonObj.Name.Equals("float"))
                {
                    setgetBuilder.AppendLine("        this." + instance + "=" + "@float" + ";");
                }
                else if (jsonObj.Name.Equals("double"))
                {
                    setgetBuilder.AppendLine("        this." + instance + "=" + "@double" + ";");
                }
                else
                {
                    setgetBuilder.AppendLine("        this." + instance + "=" + FirstCharToLower(jsonObj.Name) + ";");
                }

                setgetBuilder.AppendLine("    }");
                setgetBuilder.AppendLine();
                setgetBuilder.AppendLine("    public " + prop + " Get" + classObj + "()");
                setgetBuilder.AppendLine("    {");
                setgetBuilder.AppendLine("        return this." + instance + ";");
                setgetBuilder.AppendLine("    }");
            }
            else if (lan == LanguageEnum.PHP)
            {
                setgetBuilder.AppendLine("    public function Set" + classObj + "($" + FirstCharToLower(jsonObj.Name) + ")");
                setgetBuilder.AppendLine("    {");
                setgetBuilder.AppendLine("        $this-&gt;" + instance + "=$" + FirstCharToLower(jsonObj.Name) + ";");
                setgetBuilder.AppendLine("    }");
                setgetBuilder.AppendLine();
                setgetBuilder.AppendLine("    public function Get" + classObj + "()");
                setgetBuilder.AppendLine("    {");
                setgetBuilder.AppendLine("        return $this-&gt;" + instance + ";");
                setgetBuilder.AppendLine("    }");
            }

            return setgetBuilder.ToString();
        }

        #endregion private
    }
}