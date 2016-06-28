using Axial.Umbraco.EncryptedTextbox.Install;
using System.Configuration;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Linq;

namespace Axial.Umbraco.EncryptedTextbox.Events
{
    public class StartUpHandlers : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            PluginInstaller.InstallAppSettingKeys();
            PluginInstaller.InstallFiles();

            ContentService.Saving += ContentService_Saving;
        }

        public void ContentService_Saving(IContentService sender, SaveEventArgs<IContent> args)
        {
            foreach (var node in args.SavedEntities)
            {
                if (node.ContentType.PropertyTypes.Any(p => p.PropertyEditorAlias == "Axial.Umbraco.EncryptedTextbox"))
                {
                    foreach (var property in node.Properties)
                    {
                        if (property.PropertyType.PropertyEditorAlias.Equals("Axial.Umbraco.EncryptedTextbox"))
                        {
                            property.Value = Utilities.SimpleEncrypt.Encrypt(property.Value.ToString(), node.Key.ToString());
                        }
                    }
                }
            }
        }
    }
}
