using Newtonsoft.Json.Linq;

namespace ArcToCore.Net.Utils.Core.Interface
{
    public interface IRestClientJsonCore
    {
        JToken RestClientCore(string url);

        JToken RestContent(string content);
    }
}