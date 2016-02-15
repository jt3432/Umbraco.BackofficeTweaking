using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Axial.Umbraco.FileSystemPicker.Controllers
{
    public class FileSystemPickerEmbeddedController : Controller
    {
        public FileStreamResult Resource(string id)
        {
            var manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var resourceName = manifestResourceNames.ToList().FirstOrDefault(f => f.EndsWith(id));

            var assembly = typeof(FileSystemPickerEmbeddedController).Assembly;

            return new FileStreamResult(assembly.GetManifestResourceStream(resourceName), GetMimeType(id));
        }
        
        private string GetMimeType(string resource)
        {
            switch (Path.GetExtension(resource))
            {
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                default: return "text";
            }
        }
    }
}
