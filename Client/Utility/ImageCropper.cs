using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Client.Utility
{
    public class ImageCropper
    {
        public static MemoryStream GetCroppedImage(string fileName)
        {
            
            Bitmap source = new Bitmap(fileName);
            MemoryStream memoryStream = new MemoryStream();
            Bitmap croppedSource = null;
            if (source.Height > 1000 || source.Width > 1000)
            {
                croppedSource = source.Clone(new Rectangle(250, 250, 750, 750), source.PixelFormat);
            }
            else
            {
                croppedSource = source.Clone(new Rectangle(0, 0, source.Width, source.Height), source.PixelFormat);
            }

            switch (fileName.Substring(fileName.LastIndexOf(".")))
            {
                case ".jpg": croppedSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg); break;
                case ".png": croppedSource.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png); break;
            }

            return memoryStream;
        }
    }
}
