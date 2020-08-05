using System.Collections.Generic;

namespace ArcToCore.Net.Utils.Core.Interface
{
	public interface IConvertJsonToPoco
	{
		IEnumerable<string> CodeGenerate(string input, string culture, string nameSpaceMapping);
	}
}