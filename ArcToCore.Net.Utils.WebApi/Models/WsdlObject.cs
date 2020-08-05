using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ArcToCore.Net.Utils.WebApi.Models
{
	public class WsdlObject
	{
		public byte[] WsdlStream { get; set; }
		public string NameSpace { get; set; }
	}
}