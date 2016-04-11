using System.Linq;
using System.Net.Http.Formatting;
using Umbraco.Core.IO;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Axial.Umbraco.FileSystemPicker.Controllers;

namespace Umbraco.FileSystemPicker.Controllers
{
    [Tree("dummy", "fileSystemTree", "File System")]
    [PluginController("FileSystemPicker")]
    public class FolderSystemTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            if (!string.IsNullOrWhiteSpace(queryStrings.Get("startfolder")))
            {
                TreeNodeCollection tempTree = new TreeNodeCollection();

                var root = id == "-1" ? queryStrings.Get("startfolder") : id;
                if (id == "-1")
                {
                    var rootNode = CreateTreeNode(root, "-1", queryStrings, root.TrimStart("/"), "icon-folder", true);
                    tempTree.Add(rootNode);
                }
                else
                {
                    root = root.EnsureStartsWith("/");
                    tempTree.AddRange(AddFolders(root, queryStrings));
                    tempTree.AddRange(AddFiles(root, queryStrings));
                }                
                return tempTree;
            }

            return AddFolders(id == "-1" ? "" : id, queryStrings);
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection();

            menu.Items.Add(new MenuItem("create", "Create"));

            return menu;
        }

        private TreeNodeCollection AddFiles(string folder, FormDataCollection queryStrings)
        {
            var pickerApiController = new FileSystemPickerApiController();
            //var str = queryStrings.Get("startfolder");

            if (string.IsNullOrWhiteSpace(folder))
                return null;

            var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();


            var path = IOHelper.MapPath(folder);

            string startFolder = queryStrings.Get("startfolder");

            var rootPath = IOHelper.MapPath(startFolder.EnsureStartsWith("/"));
            var treeNodeCollection = new TreeNodeCollection();

            foreach (FileInfo fileInfo in pickerApiController.GetFiles(folder, filter))
            {
                string nodeTitle = fileInfo.Name;
                //string filePath = file.FullName.Replace(rootPath, "").Replace("\\", "/");
                string filePath = string.Format("{0}/{1}", folder, fileInfo.Name);

                TreeNode treeNode = CreateTreeNode(filePath, folder, queryStrings, nodeTitle, "icon-document", false);

                treeNodeCollection.Add(treeNode);
            }

            return treeNodeCollection;
        }

        private TreeNodeCollection AddFolders(string parent, FormDataCollection queryStrings)
        {
            var pickerApiController = new FileSystemPickerApiController();

            var filter = queryStrings.Get("filter").Split(',').Select(a => a.Trim().EnsureStartsWith(".")).ToArray();

            var treeNodeCollection = new TreeNodeCollection();

            IEnumerable<TreeNode> treeNodeList = pickerApiController.GetFolders(parent, filter)
                .Select(dir => CreateTreeNode(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"),
                    parent, queryStrings, dir.Name,
                    "icon-folder", true));

            //filter[0] == "." ?
            //            dir.EnumerateDirectories().Any() || pickerApiController.GetFiles(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any() :
            //            pickerApiController.GetFiles(dir.FullName.Replace(IOHelper.MapPath("~"), "").Replace("\\", "/"), filter).Any())

            treeNodeCollection.AddRange(treeNodeList);
            

            return treeNodeCollection;
        }
    }
}