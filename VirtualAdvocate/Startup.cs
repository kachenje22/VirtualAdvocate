using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VirtualAdvocate.Startup))]
namespace VirtualAdvocate
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
