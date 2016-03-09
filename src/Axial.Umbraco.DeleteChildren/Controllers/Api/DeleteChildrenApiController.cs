using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Axial.Umbraco.DeleteChildren.Controllers.Api
{
    [PluginController("DeleteChildren")]
    [IsBackOffice]
    public class DeleteChildrenApiController : UmbracoAuthorizedJsonController
    {
        [HttpDelete]
        public bool Delete(int parentId)
        {
            var userId = 0;
            var user = ApplicationContext.Current.Services.UserService.GetByUsername(User.Identity.Name);
            if(user != null)
            {
                userId = user.Id;
            }

            var success = true;
            try {
                var children = ApplicationContext.Current.Services.ContentService.GetChildren(parentId);

                foreach (var child in children)
                {
                    ApplicationContext.Current.Services.ContentService.Delete(child, userId);
                }

                LogHelper.Info(typeof(DeleteChildrenApiController), string.Format("Deleted child content for parent content {0}", parentId));
                
            }
            catch (Exception ex)
            {
                success = false;
                LogHelper.Error(typeof(DeleteChildrenApiController), string.Format("There was an error deleting child content for parent content {0}", parentId), ex);
            }

            return success;
        }
    }
}
