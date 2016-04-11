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

namespace Axial.Umbraco.PropertyAccess
{
    [PropertyEditor("PropertyAccess", "Property Access Editor", "/App_Plugins/PropertyAccess/")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/PropertyAccess/propertyaccess.resource.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/PropertyAccess/propertyaccess.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/PropertyAccess/Dashboard/propertyaccess.dashboard.controller.js")]

    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/PropertyAccess/propertyaccess.css")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/PropertyAccess/Dashboard/propertyaccess.dashboard.css")]
    public class PropertyAccessPropertyEditor : PropertyEditor
    {
        
    }
}
