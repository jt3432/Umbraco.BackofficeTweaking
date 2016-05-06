using Axial.Umbraco.DecendantsAudit.Install;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Axial.Umbraco.DecendantsAudit
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PluginInstaller.InstallAppSettingKeys();
            PluginInstaller.InstallFiles();

            base.ApplicationStarted(umbracoApplication, applicationContext);
        }
    }
}
