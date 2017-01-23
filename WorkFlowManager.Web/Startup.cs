using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WorkFlowManager.Web.Startup))]
namespace WorkFlowManager.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
