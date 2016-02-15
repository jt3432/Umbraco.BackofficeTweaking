using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using Umbraco.Core.Logging;

namespace Axial.Umbraco.PropertyAccess.Routes
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            const string pluginBasePath = "App_Plugins/PropertyAccess";
            string url = string.Empty;

            RouteTable.Routes.MapRoute(
                name: "PropertyAccess.GetResourcePath0",
                url: pluginBasePath + "/{resource}",
                defaults: new
                {
                    controller = "PropertyAccessEmbeddedResource",
                    action = "GetResourcePath0"
                },
                namespaces: new[] { "PropertyAccess.EmbeddedAssembly" }
            );

            RouteTable.Routes.MapRoute(
                name: "PropertyAccess.GetResourcePath1",
                url: pluginBasePath + "/{directory1}/{resource}",
                defaults: new
                {
                    controller = "PropertyAccessEmbeddedResource",
                    action = "GetResourcePath1"
                },
                namespaces: new[] { "PropertyAccess.EmbeddedAssembly" }
            );

            RouteTable.Routes.MapRoute(
                name: "PropertyAccess.GetResourcePath2",
                url: pluginBasePath + "/{directory1}/{directory2}/{resource}",
                defaults: new
                {
                    controller = "PropertyAccessEmbeddedResource",
                    action = "GetResourcePath2"
                },
                namespaces: new[] { "PropertyAccess.EmbeddedAssembly" }
            );

        }
    }
}