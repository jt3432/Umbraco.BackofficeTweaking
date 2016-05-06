using Axial.Umbraco.DecendantsAudit.Utilities;
using System.IO;
using System.Web;
using System.Xml;

namespace Axial.Umbraco.DecendantsAudit.Install
{
    public static class PluginInstaller
    {
        #region Constants

        private const string APP_VERSION_KEY = "DecendantsAudit:Version";
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
            if (Directory.Exists(@"~\App_Plugins\DecendantsAudit"))
            {
                Directory.Delete(HttpContext.Current.Server.MapPath(@"~\App_Plugins\DecendantsAudit"), true);
            }
            PluginHelpers.ExtractEmbeddedResources(@"Axial.Umbraco.DecendantsAudit.Resources", @"App_Plugins");
        }

        #endregion

    }
}
