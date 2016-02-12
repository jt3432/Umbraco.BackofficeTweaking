using ClientDependency.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Axial.Umbraco.BackofficeTweaking
{
    [PropertyEditor("BackofficeTweaking", "FileSystemPicker Editor", "/App_Plugins/BackofficeTweaking/")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/BackofficeTweaking/backofficetweaking.resource.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/BackofficeTweaking/backofficetweaking.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/BackofficeTweaking/Dashboard/backofficetweaking.dashboard.controller.js")]

    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/BackofficeTweaking/backofficetweaking.css")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/BackofficeTweaking/Dashboard/backofficetweaking.dashboard.css")]
    public class BackofficeTweakerPropEditor : PropertyEditor
    {
        
    }
}
