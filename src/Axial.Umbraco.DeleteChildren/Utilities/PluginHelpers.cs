using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using Umbraco.Web;

namespace Axial.Umbraco.DeleteChildren.Utilities
{
    public static class PluginHelpers
    {
        public static void ExtractEmbeddedResources(string resourcesRootPath, string resourceDirectoryName)
        {
            var resourceFullPath = String.Format("{0}.{1}", resourcesRootPath, resourceDirectoryName);

            var resourceNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => name.StartsWith(resourceFullPath));

            var resources = new Dictionary<string, string>();
            foreach (var resourceName in resourceNames)
            {
                var path = resourceName.Replace(resourcesRootPath, String.Empty).Replace(@".", @"\").ReplaceLastOccurrence(@"\", @".");
                resources.Add(resourceName, path);
            }

            foreach (var resource in resources)
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource.Key))
                {
                    if (stream != null)
                    {
                        var outputPath = HttpContext.Current.Server.MapPath(resource.Value);
                        FileInfo file = new FileInfo(outputPath);
                        file.Directory.Create();

                        using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create))
                        {
                            for (int i = 0; i < stream.Length; i++)
                            {
                                fileStream.WriteByte((byte)stream.ReadByte());
                            }
                            fileStream.Close();
                        }
                    }
                }
            }
        }

        public static void RemoveSetting(string key)
        {
            string result = String.Empty;
            try
            {
                var configFile = WebConfigurationManager.OpenWebConfiguration("/");
                var settings = configFile.AppSettings.Settings;
                if (settings[key] != null)
                {
                    settings.Remove(key);
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (Exception)
            {
                // Opps
            }
        }

        public static string ReadSetting(string key)
        {
            string result = String.Empty;
            try
            {
                var appSettings = WebConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
            }
            catch (Exception)
            {
                // Opps
            }

            return result;
        }

        public static bool AddUpdateAppSettings(string key, string value)
        {
            bool success = false;
            try
            {
                var configFile = WebConfigurationManager.OpenWebConfiguration("/");
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
                success = true;
            }
            catch (Exception)
            {
                // Opps
            }
            return success;
        }

        public static string ReplaceLastOccurrence(this string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }
    }
}
