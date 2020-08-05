using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArcToCore.Net.Utils.WebApi.Models
{
	public class JsonObject
	{
		public byte[] JsonBytes { get; set; }

		public string Culture { get; set; }

		public string NameSpace { get; set; }
	}
}