using System.Web.Mvc;
using System.Web.Routing;

namespace Axial.Umbraco.DecendantsAudit.Routes
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            const string pluginBasePath = "App_Plugins/DecendantsAudit";

            routes.MapRoute(
                name: "DecendantsAudit.GetResource",
                url: pluginBasePath + "/{resource}",
                defaults: new
                {
                    controller = "DecendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );

            routes.MapRoute(
                name: "DecendantsAudit.GetControllerResource",
                url: pluginBasePath + "/controllers/{resource}",
                defaults: new
                {
                    controller = "DecendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );

            routes.MapRoute(
                name: "DecendantsAudit.GetViewResource",
                url: pluginBasePath + "/views/{resource}",
                defaults: new
                {
                    controller = "DecendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );

            routes.MapRoute(
                name: "DecendantsAudit.GetVendorResource",
                url: pluginBasePath + "/vendors/{resource}",
                defaults: new
                {
                    controller = "DecendantsAuditEmbeddedResource",
                    action = "GetResource"
                }
            );
        }
    }
}