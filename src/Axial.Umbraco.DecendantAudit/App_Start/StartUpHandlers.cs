using Axial.Umbraco.DecendantsAudit.Routes;
using System.Web.Routing;
using Umbraco.Core;

namespace Axial.Umbraco.DecendantsAudit.App_Start
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            var devMode = System.Configuration.ConfigurationManager.AppSettings["Axial:FileSystemPicker:DevMode"];
            if (devMode == null || devMode.ToLower() != "true")
            {
                RouteConfig.RegisterRoutes(RouteTable.Routes);
            }
        }
    }
}
