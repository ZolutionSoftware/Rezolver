using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Rezolver.Examples.Mvc.Startup))]
namespace Rezolver.Examples.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
