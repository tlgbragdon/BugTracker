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
            if (file.ContentLength > 2*1024*1024 || file.ContentLength < 1024) { return false; }
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
}