using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.FileSystemPicker.Controllers
{
    [PluginController("FileSystemPicker")]
    public class FileSystemThumbnailApiController : UmbracoAuthorizedApiController
    {
        public HttpResponseMessage GetThumbnail(string imagePath = "", string width = "150")
        {
            width = width ?? "150";
            var imageWidth = 0;
            Int32.TryParse(width, out imageWidth);

            if (!string.IsNullOrWhiteSpace(imagePath) && imagePath.IndexOf("{{") < 0)
            {
                if (File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(imagePath)))
                {
                    using (var image = Image.FromFile(System.Web.Hosting.HostingEnvironment.MapPath(imagePath)))
                    {
                        MemoryStream outStream = new MemoryStream();

                        byte[] photoBytes = File.ReadAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(imagePath)); // change imagePath with a valid image path
                        ISupportedImageFormat format = new JpegFormat { Quality = 70 }; // convert to jpg

                        var inStream = new MemoryStream(photoBytes);

                        var imageFactory = new ImageFactory(preserveExifData: true);

                        Size size = ResizeKeepAspect(image.Size, imageWidth, imageWidth);

                        ResizeLayer resizeLayer = new ResizeLayer(size, ResizeMode.Max);
                        imageFactory.Load(inStream)
                                .Resize(resizeLayer)
                                .Format(format)
                                .Save(outStream);
                        HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                        response.Content = new StreamContent(outStream);
                        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                        return response;
                    }
                }   
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }        
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        }

        [HttpGet]
        public HttpResponseMessage ImageCheck(string imagePath = "", string width = "", string height = "")
        {
            string output = "";

            try
            {
                if (false == string.IsNullOrWhiteSpace(imagePath) && 
                    false == string.IsNullOrWhiteSpace(width) && width != "undefined" &&
                    false == string.IsNullOrWhiteSpace(height) && height != "undefined")
                {
                    string fullFilePath = IOHelper.MapPath(imagePath);

                    if (true == File.Exists(fullFilePath))
                    {
                        using (Image image = Image.FromFile(System.Web.Hosting.HostingEnvironment.MapPath(imagePath)))
                        {
                            if (image != null)
                            {
                                if (image.Width != int.Parse(width) && image.Height != int.Parse(height))
                                {
                                    output = "<br /><strong style=\"color: #F00;\">Incorrect resolution. Not " + width + "x" + height + "</strong>";
                                }
                            }
                        }
                    }

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(output);
                    return response;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<FileSystemPickerApiController>("FileSystemPicker: Image Check Error", ex);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }


        public static Size ResizeKeepAspect(Size CurrentDimensions, int maxWidth, int maxHeight)
        {
            int newHeight = CurrentDimensions.Height;
            int newWidth = CurrentDimensions.Width;
            if (maxWidth > 0 && newWidth > maxWidth) //WidthResize
            {
                Decimal divider = Math.Abs((Decimal)newWidth / (Decimal)maxWidth);
                newWidth = maxWidth;
                newHeight = (int)Math.Round((Decimal)(newHeight / divider));
            }
            if (maxHeight > 0 && newHeight > maxHeight) //HeightResize
            {
                Decimal divider = Math.Abs((Decimal)newHeight / (Decimal)maxHeight);
                newHeight = maxHeight;
                newWidth = (int)Math.Round((Decimal)(newWidth / divider));
            }
            return new Size(newWidth, newHeight);
        }
    }
}