using Microsoft.Owin;
using Owin;
using Web.SurveySystem.Helpers;

[assembly: OwinStartupAttribute(typeof(Web.SurveySystem.Startup))]
namespace Web.SurveySystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(AutofacConfig.ConfigureContainer(app));
        }
    }
}