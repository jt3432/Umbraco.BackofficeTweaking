using Axial.Umbraco.DeleteChildren.Install;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Axial.Umbraco.DeleteChildren
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PluginInstaller.InstallAppSettingKeys();
            PluginInstaller.InstallFiles();

            TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;

            base.ApplicationStarted(umbracoApplication, applicationContext);
        }

        private void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            // this example will add a custom menu item for all admin users
            // for all content tree nodes
            if (sender.TreeAlias == "content"
                && sender.Security.CurrentUser.UserType.Alias == "admin")
            {
                var deleteChildrenMenuItem = new MenuItem("deleteChildren", "Delete Children");

                deleteChildrenMenuItem.Icon = "delete";
                deleteChildrenMenuItem.Alias = "deleteChildrenMenuItem";
                deleteChildrenMenuItem.SeperatorBefore = true;
                deleteChildrenMenuItem.LaunchDialogView("/App_Plugins/DeleteChildrenMenuItem/views/delete-children-confirmation.html", "Delete Children");

                e.Menu.Items.Add(deleteChildrenMenuItem);
            }
        }
    }
}
