using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ArcToCore.Net.Utils.WebApi.Startup))]

namespace ArcToCore.Net.Utils.WebApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}