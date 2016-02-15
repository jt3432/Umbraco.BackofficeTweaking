using System;
using System.Web.Configuration;
using System.Web.Routing;
using Umbraco.Core;

namespace Axial.Umbraco.FileSystemPicker.App_Start
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //CompilationSection compilationSection = (CompilationSection)System.Configuration.ConfigurationManager.GetSection(@"system.web/compilation");

            var devMode = System.Configuration.ConfigurationManager.AppSettings["Axial:FileSystemPicker:DevMode"];
            if (devMode == null || devMode.ToLower() != "true")
            {
                RouteConfig.RegisterRoutes(RouteTable.Routes);
            }
        }
    }
}
