using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Site22.Startup))]
namespace Site22
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
