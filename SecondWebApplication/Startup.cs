using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SecondWebApplication.Startup))]
namespace SecondWebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
