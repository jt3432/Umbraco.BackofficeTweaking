using Axial.Umbraco.PropertyAccess.Handlers;
using Axial.Umbraco.PropertyAccess.Installer;
using Axial.Umbraco.PropertyAccess.Routes;
using System.Web.Routing;
using Umbraco.Core;

namespace Axial.Umbraco.PropertyAccess.Events
{
    public class UmbracoStartupEvent : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            var installHelper = new InstallHelper();
            installHelper.AddSectionDashboard();

            //// Register routes for embedded files
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Load rules and create config file if it doesn't exist
            Helpers.ConfigFileHelper.LoadAndCacheConfig();

            // Process rules
            System.Web.Http.GlobalConfiguration.Configuration.MessageHandlers.Add(new RulesHandler());
        }
    }
}