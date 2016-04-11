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

namespace Axial.Umbraco.FileSystemPicker.Editors
{
    [PropertyEditor("FileSystemPicker", "FileSystemPicker Editor", "/App_Plugins/FileSystemPicker/filesystem-picker.html", ValueType = "TEXT")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/FileSystemPicker/filesystem-picker-controller.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/FileSystemPicker/filesystem-picker-directives.js")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/FileSystemPicker/filesystem-picker-directives.css")]
    public class FileSystemPickerPropEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new FileSystemPickerPreValueEditor();
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get
            {
                return new Dictionary<string, object>
                {
                    {"filter", "png, jpg, gif"},
                    {"thumbnailSize", "150"}
                };
            }
        }

        internal class FileSystemPickerPreValueEditor : PreValueEditor
        {
            public FileSystemPickerPreValueEditor()
            {
                //create the fields
                Fields.Add(new PreValueField()
                {
                    Description = "Enable management of files only.",
                    Key = "managementMode",
                    Name = "Management Mode",
                    View = "boolean",

                });

                Fields.Add(new PreValueField()
                {
                    Description = "Pick the start folder to select a file from",
                    Key = "folder",
                    Name = "Folder",
                    //View = "/App_Plugins/FileSystemPicker/foldersystem-picker.html",
                    View = "textstring"

                });

                Fields.Add(new PreValueField()
                {
                    Description = "Comma separated list of extensions to filter the files to select from (i.e. 'png, jpg, gif'), no wildcard, no dot",
                    Key = "filter",
                    Name = "Filter",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "Enter a width for the thumbnail",
                    Key = "thumbnailSize",
                    Name = "Thumbnail width",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "Use coordinate picker for image",
                    Key = "coordinatePicker",
                    Name = "Coordinate Picker",
                    View = "boolean"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "Enter the alias of the property to use for the dynamic start folder name",
                    Key = "startFolderNamePropertyAlias",
                    Name = "Start Folder Name Property Alias",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "Remove characters from property alias for the dynamic start folder",
                    Key = "removeCharactersPropertyAlias",
                    Name = "Character Removal Property Alias",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "Enter the file image's required resolution (width)",
                    Key = "checkImageWidth",
                    Name = "Image Check Width",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "Enter the file image's required resolution (height)",
                    Key = "checkImageHeight",
                    Name = "Image Check Height",
                    View = "textstring"
                });
            }
        }
    }
}
