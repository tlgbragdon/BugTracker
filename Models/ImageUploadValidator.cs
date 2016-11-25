using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class ImageUploadValidator
    {
        public static bool IsWebFriendlyImage(HttpPostedFileBase file)
        {
            // verify file parameter is valid
            if (file == null) { return false; }
            // verify max/min size of file
            if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024) { return false; }
            try
            {
                using (var img = Image.FromStream(file.InputStream))
                {
                    return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                        ImageFormat.Png.Equals(img.RawFormat) ||
                        ImageFormat.Gif.Equals(img.RawFormat) ||
                        ImageFormat.Bmp.Equals(img.RawFormat);
                }
            }
            catch
            {
                return false;
            }
        }
    }


    public class FileUploadValidator
    {
        public static bool IsValidUploadFile(HttpPostedFileBase file)
        {
            // verify file parameter is valid
            if (file == null) { return false; }
            // verify max/min size of file
            if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024) { return false; }
            return true;
        }

        public static string FileIcon(string fileName)
        {
            string iconClass = "";

            switch (Path.GetExtension(fileName))
            {
                case ".jpg":
                case ".png":
                case ".gif":
                    // no icon - will use thumbnail image instead
                    break;

                case ".doc":
                    iconClass = "fa fa-file-word-o";
                    break;

                case ".html":
                case ".cs":
                case ".js":
                case ".css":
                    iconClass = "fa fa-file-code-o";
                    break;

                case ".pdf":
                    iconClass = "fa fa-file-pdf-o";
                    break;

                case ".avi":
                case ".mov":
                case ".wmf":
                case ".qt":
                case ".flv":
                case ".rm":
                case ".mpg":
                case ".mpeg":
                    iconClass = "fa fa-file-video-o";
                    break;

                default:
                    iconClass = "fa fa-file-text-o";
                    break;
            }
            return iconClass;
        }
    }
}