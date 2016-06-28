using Axial.Umbraco.EncryptedTextbox.Utilities;
using System.IO;
using System.Web;

namespace Axial.Umbraco.EncryptedTextbox.Install
{
    public static class PluginInstaller
    {
        #region Constants

        private const string APP_VERSION_KEY = "EncryptedTextbox:Version";
        private const string APP_VERSION_VALUE = "A.11";

        #endregion

        #region Private Variables

        #endregion

        #region Public Methods

        public static void InstallAppSettingKeys()
        {
            if (PluginHelpers.ReadSetting(APP_VERSION_KEY).Equals("Not Found"))
            {
                PluginHelpers.AddUpdateAppSettings(APP_VERSION_KEY, "Install");
            }
        }

        public static void InstallFiles()
        {
            if (!PluginHelpers.ReadSetting(APP_VERSION_KEY).Equals(APP_VERSION_VALUE))
            {
                InstallPluginFiles();
                PluginHelpers.AddUpdateAppSettings(APP_VERSION_KEY, APP_VERSION_VALUE);
            }
        }
        
        #endregion

        #region Private Methods

        private static void InstallPluginFiles()
        {
            var path = HttpContext.Current.Server.MapPath(@"~\App_Plugins\EncryptedTextbox");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            PluginHelpers.ExtractEmbeddedResources(@"Axial.Umbraco.EncryptedTextbox.Resources", @"App_Plugins");
        }

        #endregion

    }
}
