using Axial.Umbraco.DeleteChildren.Utilities;
using System.IO;
using System.Web;
using System.Xml;

namespace Axial.Umbraco.DeleteChildren.Install
{
    public static class PluginInstaller
    {
        #region Constants

        private const string APP_VERSION_KEY = "DeleteChildren:Version";
        private const string APP_VERSION_VALUE = "1.0";

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
            if (Directory.Exists(@"~\App_Plugins\DeleteChildrenMenuItem"))
            {
                Directory.Delete(HttpContext.Current.Server.MapPath(@"~\App_Plugins\DeleteChildrenMenuItem"), true);
            }
            PluginHelpers.ExtractEmbeddedResources(@"Axial.Umbraco.DeleteChildren.Resources", @"App_Plugins");
        }

        #endregion

    }
}
