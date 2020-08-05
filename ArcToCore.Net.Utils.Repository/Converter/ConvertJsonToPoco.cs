using ArcToCore.Net.Utils.Core.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArcToCore.Net.Utils.Repository.Converter
{
	public class ConvertJsonToPoco : IConvertJsonToPoco
	{
		#region Variables

		private readonly Dictionary<string, List<string>> checkClassNamesDic = new Dictionary<string, List<string>>();
		private readonly List<string> checkClassVariableNamesLst = new List<string>();
		private char[] whitespace = { ' ', '\t' };
		private List<string> codeList = new List<string>();
		private string nameSpace = string.Empty;

		private ConvertJsonToPoco.Classes classes;

		#endregion Variables

		#region Interfaces

		private readonly IOutputHelper _iOutputHelper;
		private IRestClientJsonCore _iRestClientJsonCore;
		private IObjectHandler _iObjectHandler;

		#endregion Interfaces

		public ConvertJsonToPoco(IOutputHelper iOutputHelper,
			IRestClientJsonCore iRestClientJsonCore,
			IObjectHandler iObjectHandler)
		{
			_iObjectHandler = iObjectHandler;
			_iOutputHelper = iOutputHelper;

			_iRestClientJsonCore = iRestClientJsonCore;
		}

		#region Public

		public IEnumerable<string> CodeGenerate(string input, string culture, string nameSpaceMapping)
		{
			codeList.Clear();
			nameSpace = nameSpaceMapping;
			this.GenerateCodeClasses(input, culture, nameSpace, codeList).ToString();
			return codeList;
		}

		#endregion Public

		#region Private

		private void AddJsonObject(JObject jObj, string className, string culture, string parent = "")
		{
			className = ConvertJsonToPoco.Classes.RenameCodeClass(className);
			this.AddJsonObject(jObj, ref className, culture, parent);
		}

		private void AddJsonObject(JObject jObj, ref string className, string culture, string parent = "")
		{
			className = ConvertJsonToPoco.Classes.RenameCodeClass(className);
			if (jObj == null)
			{
				return;
			}
			ConvertJsonToPoco.ClassCode existed = null;
			string old = className;
			if (!this.classes.ContainsKey(className))
			{
				this.classes.Add(className, new ConvertJsonToPoco.ClassCode()
				{
					Parent = parent
				});
			}
			else if (this.classes[className].Parent != parent)
			{
				existed = this.classes[className];
				while (this.classes.ContainsKey(className) && this.classes[className].Parent != parent)
				{
					char last = className.Last<char>();
					if (char.IsNumber(last))
					{
						className = className.Substring(0, className.Length - 1);
					}
					else
					{
						last = '0';
					}
					int num = int.Parse(last.ToString(CultureInfo.InvariantCulture)) + 1;
					className = string.Concat(className, num.ToString());
				}
				if (!this.classes.ContainsKey(className))
				{
					this.classes.Add(className, new ConvertJsonToPoco.ClassCode()
					{
						Parent = parent
					});
				}
			}
			foreach (KeyValuePair<string, JToken> item in jObj)
			{
				if (item.Value.Type == JTokenType.Object)
				{
					string name = ConvertJsonToPoco.UpperCaseFirst(item.Key);
					this.AddJsonObject(item.Value as JObject, ref name, culture, className);
					this.AddJsonProperty(className, item.Key, this.ToCsharpType(item.Value, name));
				}
				else if (item.Value.Type != JTokenType.Array)
				{
					this.AddJsonProperty(className, item.Key, this.ToCsharpType(item.Value, ""));
				}
				else
				{
					JArray array = item.Value as JArray;
					JToken first = null;
					if (array.First != null)
					{
						first = (
							from x in array
							orderby x.ToString().Length descending
							select x).First<JToken>();
					}
					string type = this.ToCsharpType(item.Value, ConvertJsonToPoco.UpperCaseFirst(ConvertJsonToPoco.Singularize(item.Key, culture)));
					if (first == null || first.Type != JTokenType.Object)
					{
						type = this.ToCsharpType(item.Value, this.ToCsharpType(first, ""));
					}
					else
					{
						string name = ConvertJsonToPoco.UpperCaseFirst(ConvertJsonToPoco.Singularize(item.Key, culture));
						this.AddJsonObject(first as JObject, ref name, culture, className);
						type = this.ToCsharpType(item.Value, name);
					}
					this.AddJsonProperty(className, item.Key, type);
				}
			}
			KeyValuePair<string, ConvertJsonToPoco.ClassCode> current = this.classes.LastOrDefault<KeyValuePair<string, ConvertJsonToPoco.ClassCode>>();
			if (!current.Value.Properties.Keys.All<string>((string x) => Regex.IsMatch(x, "^\\d")))
			{
				if (existed != null)
				{
					KeyValuePair<string, ConvertJsonToPoco.ClassCode> last = this.classes.LastOrDefault<KeyValuePair<string, ConvertJsonToPoco.ClassCode>>();
					if (last.Value.Properties.DictionaryEqual<string, string>(existed.Properties))
					{
						className = old;
						this.classes.Remove(last.Key);
					}
				}
				return;
			}
			List<IGrouping<string, KeyValuePair<string, string>>> r = (
				from x in current.Value.Properties
				group x by x.Value).ToList<IGrouping<string, KeyValuePair<string, string>>>();
			if (r.Count != 1)
			{
				className = "Dictionary<string,string>";
			}
			else
			{
				className = string.Concat("Dictionary<string,", r.First<IGrouping<string, KeyValuePair<string, string>>>().Key, ">");
			}
			this.classes.Remove(current.Key);
		}

		private void AddJsonProperty(string className, string propName, string propType)
		{
			if (!this.classes[className].Properties.ContainsKey(propName))
			{
				this.classes[className].Properties.Add(propName, propType);
				return;
			}
			string old = this.classes[className].Properties[propName];
			if (old != propType && (old == "object" || old == "Undefined"))
			{
				this.classes[className].Properties[propName] = propType;
			}
		}

		private ConvertJsonToPoco.Classes GenerateCodeClasses(string input, string culture, string nameSpace, List<string> codeList)
		{
			this.classes = new ConvertJsonToPoco.Classes(nameSpace, codeList);

			try
			{
				object obj = JsonConvert.DeserializeObject(input);
				if (obj is JObject)
				{
					this.AddJsonObject(obj as JObject, "RootObject", culture, "");
				}
				else if (obj is JArray)
				{
					this.AddJsonObject((obj as JArray).First as JObject, "RootObject", culture, "");
				}
			}
			catch (Exception ex)
			{
				throw new JsonReaderException("Deserialization failed: " + "\n\n" + input);
			}
			return this.classes;
		}

		private void GetAllJsonObject(JToken token, List<JObject> t)
		{
			if (token is JObject)
			{
				t.Add(token as JObject);
				foreach (KeyValuePair<string, JToken> keyValuePair in token as JObject)
				{
				}
			}
			if (token is JArray)
			{
				this.GetAllJsonObject((token as JArray).First, t);
			}
		}

		private static string Singularize(string input, string culture)
		{
			return PluralizationService.CreateService(CultureInfo.GetCultureInfo(culture)).Singularize(input);
		}

		private string ToCsharpType(JToken token, string objType = "")
		{
			switch ((token != null ? token.Type : JTokenType.Null))
			{
				case JTokenType.None:
				case JTokenType.Constructor:
				case JTokenType.Property:
				case JTokenType.Comment:
				case JTokenType.Undefined:
				case JTokenType.Raw:
				case JTokenType.Guid:
				case JTokenType.Uri:
					{
						return "Undefined";
					}
				case JTokenType.Object:
					{
						if (objType != "")
						{
							return objType;
						}
						return "object";
					}
				case JTokenType.Array:
					{
						//"&lt;string&gt;";
						return string.Format("List&lt;{0}&gt;", objType);
					}
				case JTokenType.Integer:
					{
						if (token.ToObject<long>() < (long)2147483647)
						{
							return "int";
						}
						return "long";
					}
				case JTokenType.Float:
					{
						return "double";
					}
				case JTokenType.String:
					{
						return "string";
					}
				case JTokenType.Boolean:
					{
						return "bool";
					}
				case JTokenType.Null:
					{
						return "object";
					}
				case JTokenType.Date:
					{
						return "DateTime";
					}
				case JTokenType.Bytes:
					{
						return "byte[]";
					}
				case JTokenType.TimeSpan:
					{
						return "TimeSpan";
					}
			}
			return "Undefined";
		}

		private static string UpperCaseFirst(string input)
		{
			return string.Format("{0}{1}", input.Substring(0, 1).ToUpper(), input.Remove(0, 1));
		}

		private class ClassCode
		{
			public string Parent
			{
				get;
				set;
			}

			public Dictionary<string, string> Properties
			{
				get;
				set;
			}

			public ClassCode()
			{
				this.Properties = new Dictionary<string, string>();
			}
		}

		private class Classes : Dictionary<string, ConvertJsonToPoco.ClassCode>
		{
			private static string _nameSpace;
			private static List<string> _codeList;

			public Classes(string nameSpace, List<string> codeList)
			{
				_nameSpace = nameSpace;
				_codeList = codeList;
			}

			private static IEnumerable<string> CreateClassString(KeyValuePair<string, ConvertJsonToPoco.ClassCode> c)
			{
				StringBuilder stringBuilder = new StringBuilder();

				if (!string.IsNullOrEmpty(_nameSpace))
				{
					stringBuilder.Append("namespace " + _nameSpace + "\r\n");
					stringBuilder.Append("{\r\n");
				}

				stringBuilder.Append("using System;\r\n");
				stringBuilder.Append("using System.Collections.Generic;\r\n");
				stringBuilder.Append("using Newtonsoft.Json;\r\n");
				stringBuilder.AppendFormat("public class {0}\r\n", c.Key);
				stringBuilder.Append("{\r\n");
				foreach (KeyValuePair<string, string> p in c.Value.Properties)
				{
					string pName = ConvertJsonToPoco.Classes.RenameCodeClass(p.Key);
					MatchCollection matches = Regex.Matches(stringBuilder.ToString(), string.Concat("public\\s(.*?)\\s", pName, ";"));
					if (matches != null && matches.Count > 0)
					{
						pName = string.Concat(pName, matches.Count);
					}

					string propName = string.Empty;
					propName = p.Value;
					if (c.Key.Equals(pName))
					{
						pName = pName + "1";
					}

					stringBuilder.AppendFormat("\t[JsonProperty(\"{0}\")]\r\n\tpublic {1} {2} {{ get; set; }}\r\n", p.Key, p.Value, pName);
				}
				stringBuilder.Append("}\r\n");

				if (!string.IsNullOrEmpty(_nameSpace))
				{
					stringBuilder.Append("}\r\n");
				}

				stringBuilder.AppendLine();

				_codeList.Add(stringBuilder.ToString());

				return _codeList;
			}

			public static string RenameCodeClass(string input)
			{
				string result = "";
				result = Regex.Replace(input, "[^\\w_-]", "");
				if (result.Contains<char>('-'))
				{
					string str = "";
					result.Split(new char[] { '-' }).ToList<string>().ForEach((string x) =>
					{
						if (x.Length > 1)
						{
							str = string.Concat(str, ConvertJsonToPoco.UpperCaseFirst(x));
						}
					});
					result = str;
				}
				if (result.Contains<char>('\u005F'))
				{
					string str1 = "";
					result.Split(new char[] { '\u005F' }).ToList<string>().ForEach((string x) =>
					{
						if (x.Length > 1)
						{
							str1 = string.Concat(str1, ConvertJsonToPoco.UpperCaseFirst(x));
						}
					});
					result = str1;
				}
				return ConvertJsonToPoco.UpperCaseFirst(result);
			}

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, ConvertJsonToPoco.ClassCode> c in this)
				{
					stringBuilder.Append(ConvertJsonToPoco.Classes.CreateClassString(c));
				}
				return stringBuilder.ToString();
			}
		}

		 

		#endregion Private
	}

	public static class DeepCodeExtension
	{
		public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second)
		{
			return first.DictionaryEqual<TKey, TValue>(second, null);
		}

		public static bool DictionaryEqual<TKey, TValue>(this IDictionary<TKey, TValue> first, IDictionary<TKey, TValue> second, IEqualityComparer<TValue> valueComparer)
		{
			TValue secondValue;
			bool flagState;
			if (first == second)
			{
				return true;
			}
			if (first == null || second == null)
			{
				return false;
			}
			if (first.Count != second.Count)
			{
				return false;
			}
			object @default = valueComparer;
			if (@default == null)
			{
				@default = EqualityComparer<TValue>.Default;
			}
			valueComparer = (IEqualityComparer<TValue>)@default;
			using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = first.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> kvp = enumerator.Current;
					if (second.TryGetValue(kvp.Key, out secondValue))
					{
						if (valueComparer.Equals(kvp.Value, secondValue))
						{
							continue;
						}
						flagState = false;
						return flagState;
					}
					else
					{
						flagState = false;
						return flagState;
					}
				}
				return true;
			}
			 
		}
	}
}