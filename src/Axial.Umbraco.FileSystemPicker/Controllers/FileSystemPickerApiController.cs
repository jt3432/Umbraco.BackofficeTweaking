using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using Umbraco.Web.WebApi;
//test
namespace Axial.Umbraco.FileSystemPicker.Controllers
{
    [PluginController("FileSystemPicker")]
    public class FileSystemPickerApiController : UmbracoAuthorizedJsonController
    {
        public object GetStartFolderName(string currentNodeId, string startFolderNamePropertyAlias, string removeCharactersPropertyAlias)
        {
            var startFolderName = String.Empty;
            var contentService = Services.ContentService;

            int id = 0;
            if (Int32.TryParse(currentNodeId, out id))
            {
                var node = contentService.GetById(id);
                if (node != null)
                {
                    if (false == string.IsNullOrEmpty(startFolderNamePropertyAlias))
                    {
                        while (true == string.IsNullOrEmpty(startFolderName) && node.Level > 0)
                        {
                            if (node.HasProperty(startFolderNamePropertyAlias) && false == string.IsNullOrEmpty(node.GetValue<string>(startFolderNamePropertyAlias)))
                            {
                                startFolderName = node.GetValue<string>(startFolderNamePropertyAlias);
                            }
                            else
                            {
                                node = contentService.GetById(node.ParentId);
                            }
                        }

                        if (false == string.IsNullOrWhiteSpace(startFolderName) && false == string.IsNullOrWhiteSpace(removeCharactersPropertyAlias))
                        {
                            startFolderName = Regex.Replace(startFolderName, removeCharactersPropertyAlias, "");
                        }
                    }
                }
            }
            return new { folderName = startFolderName };
        }

        public IEnumerable<DirectoryInfo> GetFolders(string folder, string[] filter)
        {
            var path = IOHelper.MapPath(folder.TrimStart(new char[] { '~', '/' }).EnsureStartsWith("~/"));

            return new DirectoryInfo(path).GetDirectories("*");
        }

        public IEnumerable<FileInfo> GetFiles(string folder, string[] filter)
        {
            var path = IOHelper.MapPath("~/" + folder.TrimStart('~', '/'));

            var files = default(IEnumerable<FileInfo>);

            if (Directory.Exists(path))
            {
                files = new DirectoryInfo(path).GetFiles();

                if (filter != null && filter[0] != ".")
                {
                    files = files.Where(f => filter.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));
                }
            }

            return files;
        }

        public HttpResponseMessage PostCreateFolder(string folderPath)
        {
            var path = IOHelper.MapPath(folderPath.EnsureStartsWith("~/"));
            var dirInfo = Directory.CreateDirectory(path);

            return Request.CreateResponse(HttpStatusCode.OK, dirInfo);
        }

        public async Task<HttpResponseMessage> PostAddFile()
        {
            if (Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            //must have a file
            if (result.FileData.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //get the string json from the request
            var parentDir = IOHelper.MapPath(result.FormData["currentFolder"].ToString());
            if (Directory.Exists(parentDir) == false)
            {
                return Request.CreateValidationErrorResponse("The request was not formatted correctly, the currentFolder is not a valid path");
            }

            var tempFiles = new PostedFiles();

            //get the files
            foreach (var file in result.FileData)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' });
                var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();

                if (UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(ext) == false)
                {
                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                    {
                        fileName = fileName.Trim('"');
                    }
                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                    {
                        fileName = Path.GetFileName(fileName);
                    }

                    try
                    {
                        File.Move(file.LocalFileName, Path.Combine(parentDir, fileName));

                        tempFiles.UploadedFiles.Add(new ContentItemFile
                        {
                            FileName = fileName,
                            PropertyAlias = Constants.Conventions.Media.File,
                            TempFilePath = file.LocalFileName
                        });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(typeof(FileSystemPickerApiController), ex.Message, ex);
                        AddCancelMessage(tempFiles,
                            message: Services.TextService.Localize("speechBubbles/operationCancelledText", System.Globalization.CultureInfo.CurrentCulture, null) + " -- " + fileName,
                            localizeMessage: false);
                    }
                }
                else
                {
                    tempFiles.Notifications.Add(new Notification(
                        Services.TextService.Localize("speechBubbles/operationFailedHeader", System.Globalization.CultureInfo.CurrentCulture, null),
                        "Cannot upload file " + file.Headers.ContentDisposition.FileName + ", it is not an approved file type",
                        SpeechBubbleIcon.Warning));
                }
            }

            //Different response if this is a 'blueimp' request
            if (Request.GetQueryNameValuePairs().Any(x => x.Key == "origin"))
            {
                var origin = Request.GetQueryNameValuePairs().First(x => x.Key == "origin");
                if (origin.Value == "blueimp")
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        tempFiles,
                        //Don't output the angular xsrf stuff, blue imp doesn't like that
                        new JsonMediaTypeFormatter());
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, tempFiles);
        }

        public HttpResponseMessage PostRename(string path, string name)
        {
            path = IOHelper.MapPath("~/" + path.TrimStart('~', '/'));
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                file.MoveTo(String.Format("{0}\\{1}{2}", file.Directory.FullName, name, file.Extension));
                return Request.CreateResponse(HttpStatusCode.OK, file);
            }
            else
            {
                var dir = new DirectoryInfo(path);
                dir.MoveTo(String.Format("{0}\\{1}", dir.Parent.FullName, name));
                return Request.CreateResponse(HttpStatusCode.OK, dir);
            }
            
        }

        public HttpResponseMessage PostDelete(string path)
        {
            path = IOHelper.MapPath("~/" + path.TrimStart('~', '/'));
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                file.Delete();
            }
            else
            {
                var dir = new DirectoryInfo(path);
                if(dir.GetFiles().Count() == 0)
                {
                    dir.Delete();
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public Dictionary<string, string> GetCommonConfig()
        {
            var config = new Dictionary<string, string>();

            var startFolder = System.Configuration.ConfigurationManager.AppSettings["Axial:FileSystemPicker:startFolder"];
            var startFolderNamePropertyAlias = System.Configuration.ConfigurationManager.AppSettings["Axial:FileSystemPicker:startFolderNamePropertyAlias"];

            config.Add("startFolder", startFolder);
            config.Add("startFolderNamePropertyAlias", startFolderNamePropertyAlias);

            return config;
        }

        protected void AddCancelMessage(INotificationModel display,
            string header = "speechBubbles/operationCancelledHeader",
            string message = "speechBubbles/operationCancelledText",
            bool localizeHeader = true,
            bool localizeMessage = true)
        {
            display.AddWarningNotification(
                localizeHeader ? Services.TextService.Localize(header, System.Globalization.CultureInfo.CurrentCulture, null) : header,
                localizeMessage ? Services.TextService.Localize(message, System.Globalization.CultureInfo.CurrentCulture, null) : message);
        }
    }


    /// <summary>
    /// This is used for the response of PostAddFile so that we can analyze the response in a filter and remove the 
    /// temporary files that were created.
    /// </summary>
    public class PostedFiles : IHaveUploadedFiles, INotificationModel
    {
        public PostedFiles()
        {
            UploadedFiles = new List<ContentItemFile>();
            Notifications = new List<Notification>();
        }
        public List<ContentItemFile> UploadedFiles { get; private set; }

        public List<Notification> Notifications { get; private set; }
    }
}