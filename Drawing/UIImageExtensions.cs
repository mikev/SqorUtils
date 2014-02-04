#if MONOTOUCH

using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using Sqor.Utils.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ImageIO;
using Sqor.Utils.Data;

namespace Sqor.Utils.Drawing
{
    public static class UIImageExtensions
    {
        public static byte[] SaveToByteArray(this UIImage image)
        {
            using (var asPng = image.AsJPEG())
            {
                return asPng.ToByteArray();
            }
        }
    
        public static UIImage ScaleClean(this UIImage image, Size newSize)
        {
            using (var bitmap = Bitmap.ScaleFrom(image.CGImage, newSize.Scale(image.CurrentScale)))
            {
                return UIImage.FromImage(bitmap.ToImage(), image.CurrentScale, image.Orientation);
            }
        }
        
        public static UIImage ScaleCleanToHeight(this UIImage image, int maximumHeight)
        {
            var newWidth = (int)(Math.Max(image.Size.Height, maximumHeight) / image.Size.Height * image.Size.Width);
            using (var bitmap = Bitmap.ScaleFrom(image.CGImage, new Size(newWidth, maximumHeight)))
            {
                return UIImage.FromImage(bitmap.ToImage(), image.CurrentScale, image.Orientation);
            }
        }
        
        public static UIImage ScaleCleanToWidth(this UIImage image, int maximumWidth)
        {
            var newHeight = (int)(Math.Max(image.Size.Width, maximumWidth) / image.Size.Width * image.Size.Height);
            using (var bitmap = Bitmap.ScaleFrom(image.CGImage, new Size(maximumWidth, newHeight)))
            {
                return UIImage.FromImage(bitmap.ToImage(), image.CurrentScale, image.Orientation);
            }
        }
        
        public static int GetWidth(this UIImage image)
        {
            return (int)(image.CGImage.Width / image.CurrentScale);
        }
        
        public static int GetHeight(this UIImage image)
        {
            return (int)(image.CGImage.Height / image.CurrentScale);
        }
    }
}

#endif