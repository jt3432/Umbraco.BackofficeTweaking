using ClientDependency.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Axial.Umbraco.DecendantsAudit
{
    [PropertyEditor("DecendantsAudit", "Decendants Audit", "/App_Plugins/DecendantsAudit/views/decendantsaudit.property.html", ValueType = "TEXT")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/DecendantsAudit/decendantsaudit.resource.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/DecendantsAudit/controllers/decendantsaudit.config.controller.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/DecendantsAudit/controllers/decendantsaudit.property.controller.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/DecendantsAudit/vendors/jquery.treetable.js")]

    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/DecendantsAudit/decendantsaudit.css")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/DecendantsAudit/vendors/jquery.treetable.css")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/DecendantsAudit/vendors/jquery.treetable.theme.default.css")]
    public class DecendantsAuditPropertyEditor : PropertyEditor
    {
        
    }
}
