using Axial.Umbraco.BackofficeTweaking.Handlers;
using Axial.Umbraco.BackofficeTweaking.Installer;
using Axial.Umbraco.BackofficeTweaking.Routes;
using System.Web.Routing;
using Umbraco.Core;

namespace Axial.Umbraco.BackofficeTweaking.Events
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