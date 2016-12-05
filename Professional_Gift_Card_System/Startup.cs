using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Professional_Gift_Card_System.Startup))]
namespace Professional_Gift_Card_System
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
