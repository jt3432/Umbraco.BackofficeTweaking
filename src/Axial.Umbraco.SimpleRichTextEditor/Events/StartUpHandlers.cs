using Axial.Umbraco.SimpleRichTextEditor.Install;
using Umbraco.Core;

namespace Axial.Umbraco.SimpleRichTextEditor.Events
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PluginInstaller.InstallAppSettingKeys();
            PluginInstaller.InstallFiles();
        }
    }
}
