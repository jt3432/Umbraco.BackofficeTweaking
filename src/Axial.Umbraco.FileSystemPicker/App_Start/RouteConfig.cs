using System.Web.Mvc;
using System.Web.Routing;

namespace Axial.Umbraco.FileSystemPicker.App_Start
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "FileSystemPicker",
                url: "App_Plugins/FileSystemPicker/{id}",
                defaults: new { controller = "FileSystemPickerEmbedded", action = "Resource", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "FileSystemPickerMcePlugin",
                url: "umbraco/lib/tinymce/plugins/fsmediapicker/{id}",
                defaults: new { controller = "FileSystemPickerEmbedded", action = "Resource", id = UrlParameter.Optional }
            );
        }
    }
}
