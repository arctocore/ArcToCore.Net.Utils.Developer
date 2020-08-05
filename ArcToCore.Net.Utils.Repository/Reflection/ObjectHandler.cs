using ArcToCore.Net.Utils.Core.Enums;
using ArcToCore.Net.Utils.Core.Interface;
using Ganss.XSS;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services.Description;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ArcToCore.Net.Utils.Repository.Reflection
{
    public class ObjectHandler : IObjectHandler
    {
        #region Variables

        private StringBuilder strWsdl = new StringBuilder
        {
            Length = 0,
            Capacity = 0
        };

        #endregion Variables

        #region public

        public string GetFreindlyTypeName(LanguageEnum lan, JProperty value)
        {
            string typeName = string.Empty;

            switch (lan)
            {
                case LanguageEnum.CSharp:
                    typeName = ExtractLanTypes(lan, value);
                    break;

                case LanguageEnum.PHP:
                    typeName = ExtractLanTypes(lan, value);
                    break;

                case LanguageEnum.Java:
                    typeName = ExtractLanTypes(lan, value);
                    break;
            }

            return typeName;
        }

        public void WalkNode(JToken node, Action<JObject> action)
        {
            if (node.Type == JTokenType.Object)
            {
                action((JObject)node);

                foreach (JProperty child in node.Children<JProperty>())
                {
                    WalkNode(child.Value, action);
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    WalkNode(child, action);
                }
            }
        }

        public Assembly GetServiceInspector(Stream stream)
        {
            var wsdlRequestStream = stream;

            var sd = ServiceDescription.Read(wsdlRequestStream);
            var sdImport = new ServiceDescriptionImporter();
            sdImport.AddServiceDescription(sd, String.Empty, String.Empty);
            sdImport.ProtocolName = "Soap";
            sdImport.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;
            var codeNameSpace = new CodeNamespace();
            var codeCompileUnit = new CodeCompileUnit();
            codeCompileUnit.Namespaces.Add(codeNameSpace);
            var warnings = sdImport.Import(codeNameSpace, codeCompileUnit);

            if (warnings == 0)
            {
                var stringWriter = new StringWriter(System.Globalization.CultureInfo.CurrentCulture);
                var prov = new Microsoft.CSharp.CSharpCodeProvider();
                prov.GenerateCodeFromNamespace(codeNameSpace, stringWriter, new CodeGeneratorOptions());
                //Compile the assembly
                var assemblyReferences = new string[2] { "System.Web.Services.dll", "System.Xml.dll" };
                var param = new CompilerParameters(assemblyReferences)
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                    TreatWarningsAsErrors = false,
                    WarningLevel = 4
                };
                var results = new CompilerResults(new TempFileCollection());
                results = prov.CompileAssemblyFromDom(param, codeCompileUnit);
                var assembly = results.CompiledAssembly;

                return assembly;
            }
            else
            {
                return null;
            }
        }

        public List<string> GenerateDtoAsString(Stream source, string nameSpace)
        {
            List<string> csharpFilesAsStringList = new List<string>();

            var assembly = GetServiceInspector(source);
            var strCode = new StringBuilder();
            foreach (var pi in assembly.DefinedTypes)
            {
                if (!string.IsNullOrEmpty(nameSpace))
                {
                    strCode.AppendLine("namespace " + nameSpace);
                    strCode.AppendLine("{");
                }

                strCode.AppendLine("");
                strCode.AppendLine("  using System;");
                strCode.AppendLine("  using System.Net;");
                strCode.AppendLine("  using System.Security.Cryptography.X509Certificates;");
                strCode.AppendLine("  using System.Web.Services.Protocols;");
                strCode.AppendLine("  using System.ComponentModel;");
                strCode.AppendLine("  using System.Text;");

                strCode.AppendLine("");

                strCode.AppendLine("  public class " + pi.Name);

                strCode.AppendLine("  {");

                foreach (PropertyInfo propertyInfoItem in pi.GetProperties())
                {
                    var propertyValue = propertyInfoItem.PropertyType.Name;
                    string propertyName = propertyInfoItem.Name;

                    if (CheckForArrayNotation(propertyValue, out var position))
                    {
                        strCode.AppendLine("       public " + propertyValue.Insert(position, "") + " " + propertyName + " { get; set; }");
                    }
                    else if (pi.GetProperties().Any(x => x.Name.Equals(propertyValue)))
                    {
                        strCode.AppendLine("       public " + propertyValue + " " + propertyName + " { get; set; }");
                    }
                    else
                    {
                        strCode.AppendLine("       public " + propertyValue + " " + propertyName + " { get; set; }");
                    }
                }

                strCode.AppendLine("  }");

                if (!string.IsNullOrEmpty(nameSpace))
                {
                    strCode.AppendLine("}");
                }

                csharpFilesAsStringList.Add(strCode.ToString());
                strCode.Clear();
            }

            return csharpFilesAsStringList;
        }

        public List<string> ReturnOperationsParameters(string source)
        {
            List<string> soapDataList = new List<string>();
            soapDataList.Clear();

            string[] lines = source
                .Split(Environment.NewLine.ToCharArray())
                .Skip(5)
                .ToArray();

            string output = string.Join(Environment.NewLine, lines);

            output = output.Remove(output.TrimEnd().LastIndexOf(Environment.NewLine, StringComparison.Ordinal));

            var resultString = Regex.Replace(output, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

            var wsdlMe = XDocumentFromWsdl(resultString);
            if (string.IsNullOrEmpty(wsdlMe))
            {
                wsdlMe = "No methods and parameters found!";
            }
            var sanitizer = new HtmlSanitizer();
            var sanitized = sanitizer.Sanitize(wsdlMe);
            soapDataList.Add(sanitized);

            return soapDataList;
        }

        #endregion public

        #region private

        private string ExtractLanTypes(LanguageEnum lan, JProperty value)
        {
            string typeName = string.Empty;
            switch (value.Value.Type)
            {
                case JTokenType.Object:

                    if (lan == LanguageEnum.CSharp || lan == LanguageEnum.Java)
                    {
                        typeName = "object";
                    }
                    break;

                case JTokenType.Array:

                    if (lan == LanguageEnum.CSharp)
                    {
                        typeName = "List";
                    }
                    else if (lan == LanguageEnum.Java)
                    {
                        typeName = "ArrayList";
                    }
                    else if (lan == LanguageEnum.PHP)
                    {
                        typeName = "array";
                    }

                    break;

                case JTokenType.Null:
                    if (lan == LanguageEnum.CSharp
                        || lan == LanguageEnum.Java
                        || lan == LanguageEnum.PHP)
                    {
                        typeName = "null";
                    }

                    break;

                case JTokenType.Integer:
                    if (lan == LanguageEnum.CSharp
                        || lan == LanguageEnum.Java
                        || lan == LanguageEnum.PHP)
                    {
                        typeName = "int";
                    }
                    break;

                case JTokenType.Float:
                    if (lan == LanguageEnum.CSharp
                        || lan == LanguageEnum.Java
                        || lan == LanguageEnum.PHP)
                    {
                        typeName = "float";
                    }
                    break;

                case JTokenType.String:
                    if (lan == LanguageEnum.CSharp)
                    {
                        typeName = "string";
                    }
                    else if (lan == LanguageEnum.Java || lan == LanguageEnum.PHP)
                    {
                        typeName = "String";
                    }

                    break;

                case JTokenType.Boolean:
                    if (lan == LanguageEnum.CSharp)
                    {
                        typeName = "bool";
                    }
                    else if (lan == LanguageEnum.Java
                        || lan == LanguageEnum.PHP)
                    {
                        typeName = "boolean";
                    }
                    break;

                case JTokenType.Date:

                    if (lan == LanguageEnum.CSharp
                        || lan == LanguageEnum.PHP)
                    {
                        typeName = "DateTime";
                    }
                    else if (lan == LanguageEnum.Java)
                    {
                        typeName = "Date";
                    }

                    break;

                case JTokenType.Bytes:
                    if (lan == LanguageEnum.CSharp || lan == LanguageEnum.Java)
                    {
                        typeName = "byte[]";
                    }
                    else if (lan == LanguageEnum.PHP)
                    {
                        typeName = "$byteArray";
                    }
                    break;

                case JTokenType.Guid:

                    if (lan == LanguageEnum.CSharp)
                    {
                        typeName = "GUID";
                    }
                    else if (lan == LanguageEnum.Java)
                    {
                        typeName = "UUID";
                    }
                    else if (lan == LanguageEnum.PHP)
                    {
                        typeName = "com_create_guid()";
                    }

                    break;

                case JTokenType.Uri:
                    if (lan == LanguageEnum.CSharp || lan == LanguageEnum.Java)
                    {
                        typeName = "Uri";
                    }
                    else if (lan == LanguageEnum.PHP)
                    {
                        typeName = "$url";
                    }
                    break;

                case JTokenType.TimeSpan:

                    if (lan == LanguageEnum.CSharp)
                    {
                        typeName = "TimeSpan";
                    }
                    else if (lan == LanguageEnum.Java)
                    {
                        typeName = "Duration";
                    }
                    else if (lan == LanguageEnum.PHP)
                    {
                        typeName = "DateInterval";
                    }

                    break;

                default:
                    throw new System.Exception("Type not recognized");
            }

            return typeName;
        }

        private string XDocumentFromWsdl(string xml)
        {
            XDocument doc = XDocument.Parse(xml);
            XNamespace wsdl = "http://schemas.xmlsoap.org/wsdl/";
            XNamespace s = "http://www.w3.org/2001/XMLSchema";

            var schema = doc.Root
                .Element(wsdl + "types")
                .Element(s + "schema");

            if (schema != null)
            {
                var elements = schema.Elements(s + "element");
                Func<XElement, string> getName = (el) => el.Attribute("name").Value;

                // these are all method names
                var names = from el in elements
                            let name = getName(el)
                            where !name.EndsWith("Response")
                            select name;

                foreach (var name in names)
                {
                    var method = elements.Single(el => getName(el) == name);

                    strWsdl.AppendLine("Method: " + name);

                    //// these are all parameters for a given method
                    var parameters = from par in method.Descendants(s + "element")
                                     select par;

                    if (parameters.Count() > 0)
                    {
                        foreach (XElement parameter in parameters)
                        {
                            string paramName = string.Empty;
                            string paramType = string.Empty;

                            try
                            {
                                paramName = parameter.Attribute("name").Value;
                            }
                            catch { }

                            try
                            {
                                paramType = parameter.Attribute("type").Value;
                            }
                            catch { }

                            if (!string.IsNullOrEmpty(paramName) && !string.IsNullOrEmpty(paramType))
                            {
                                strWsdl.AppendLine("ParamName: " + paramName + " ParamType: " + paramType);
                            }
                        }
                        strWsdl.AppendLine();
                    }
                }
            }

            return strWsdl.ToString();
        }

        private void OutputElements(XmlSchemaParticle particle)
        {
            if (particle != null)
            {
                XmlSchemaSequence sequence = particle as XmlSchemaSequence;
                XmlSchemaChoice choice = particle as XmlSchemaChoice;
                XmlSchemaAll all = particle as XmlSchemaAll;

                if (sequence != null)
                {
                    strWsdl.AppendLine("  -Sequence");

                    if (sequence.Items.Count > 0)
                    {
                        for (int i = 0; i < sequence.Items.Count; i++)
                        {
                            XmlSchemaElement childElement = sequence.Items[i] as XmlSchemaElement;
                            XmlSchemaSequence innerSequence = sequence.Items[i] as XmlSchemaSequence;
                            XmlSchemaChoice innerChoice = sequence.Items[i] as XmlSchemaChoice;
                            XmlSchemaAll innerAll = sequence.Items[i] as XmlSchemaAll;

                            if (childElement != null)
                            {
                                strWsdl.AppendLine("    --Element/Type: " + childElement.Name + " Type: " + childElement.SchemaTypeName.Name);
                            }
                            else
                            {
                                OutputElements(sequence.Items[i] as XmlSchemaParticle);
                            }
                        }
                    }
                }
                else if (choice != null)
                {
                    strWsdl.Append("  -Choice");

                    if (choice.Items != null && choice.Items.Count > 0)
                    {
                        for (int i = 0; i < choice.Items.Count; i++)
                        {
                            XmlSchemaElement childElement = choice.Items[i] as XmlSchemaElement;
                            XmlSchemaSequence innerSequence = choice.Items[i] as XmlSchemaSequence;
                            XmlSchemaChoice innerChoice = choice.Items[i] as XmlSchemaChoice;
                            XmlSchemaAll innerAll = choice.Items[i] as XmlSchemaAll;

                            if (childElement != null)
                            {
                                strWsdl.AppendLine("    --Element/Type: " + childElement.Name + " Type: " + childElement.SchemaTypeName.Name);
                            }
                            else
                            {
                                OutputElements(choice.Items[i] as XmlSchemaParticle);
                            }
                        }

                        strWsdl.AppendLine();
                    }
                }
                else if (all != null)
                {
                    strWsdl.AppendLine("  -All");

                    if (all.Items != null && all.Items.Count > 0)
                    {
                        for (int i = 0; i < all.Items.Count; i++)
                        {
                            XmlSchemaElement childElement = all.Items[i] as XmlSchemaElement;
                            XmlSchemaSequence innerSequence = all.Items[i] as XmlSchemaSequence;
                            XmlSchemaChoice innerChoice = all.Items[i] as XmlSchemaChoice;
                            XmlSchemaAll innerAll = all.Items[i] as XmlSchemaAll;

                            if (childElement != null)
                            {
                                strWsdl.AppendLine("    --Element/Type: " + childElement.Name + " Type: " + childElement.SchemaTypeName.Name);
                            }
                            else
                            {
                                OutputElements(all.Items[i] as XmlSchemaParticle);
                            }
                        }
                        strWsdl.AppendLine();
                    }
                }
            }
        }

        private bool CheckForArrayNotation(string str, out int position)
        {
            position = -1;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '[')
                {
                    position = i;
                    return true;
                }
            }
            return false;
        }

        #endregion private
    }
}