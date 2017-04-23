using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Game001WebApplication.Startup))]
namespace Game001WebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
