using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Sqor.Utils.Images
{
    public static class ImageExtensions
    {
        public static Image Scale(this Image image, int newWidth, int newHeight)
        {
            var scaled = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(scaled)) 
            {
                graphics.DrawImage(image, new Rectangle(0, 0, scaled.Width, scaled.Height));
            }
            return scaled;
        }

        public static Image Scale(this Image image, int newSize)
        {
            var width = image.Width >= image.Height ? newSize : (int)(newSize * ((float)image.Width / image.Height));
            var height = image.Height >= image.Width ? newSize : (int)(newSize * ((float)image.Height / image.Width));

            var scaled = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(scaled)) 
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, new Rectangle(0, 0, scaled.Width, scaled.Height));
            }
            return scaled;
        }

        public static Image ScaleWidth(this Image image, int newWidth)
        {
            var width = newWidth;
            var height = (int)(newWidth * ((float)image.Height / image.Width));

            var scaled = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(scaled)) 
            {
                graphics.DrawImage(image, new Rectangle(0, 0, scaled.Width, scaled.Height));
            }
            return scaled;
        }

        public static string GetFileExtension(this Image image)
        {
            if (ImageFormat.Jpeg.Equals(image.RawFormat))
            {
                return "jpg";
            }
            else if (ImageFormat.Png.Equals(image.RawFormat))
            {
                return "png";
            }
            else if (ImageFormat.Gif.Equals(image.RawFormat))
            {
                return "gif";
            }
            else
            {
                return "foo";
            }
        }

        public static byte[] SaveToBytes(this Image image, ImageFormat imageFormat = null)
        {
            imageFormat = ImageFormat.Png;
            using (var stream = new MemoryStream())
            {
                if (imageFormat == null && imageFormat.Equals(ImageFormat.Jpeg))
                {
                    using (var encoderParameters = new EncoderParameters(1))
                    {
                        var imageCodecInfo = ImageCodecInfo.GetImageEncoders().First(encoder => String.Compare(encoder.MimeType, "image/jpeg", StringComparison.OrdinalIgnoreCase) == 0);
                        var memoryStream = new MemoryStream();
                        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                        image.Save(memoryStream, imageCodecInfo, encoderParameters);
                        
                        return memoryStream.ToArray();
                    }
                }
                else
                {
                    image.Save(stream, imageFormat ?? image.RawFormat);
                }
                return stream.ToArray();
            }
        }

        public static Image Crop(this Image image, int x, int y, int width, int height, int newWidth, int newHeight)
        {
            var scaled = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(scaled)) 
            {
                graphics.DrawImage(
                    image, 
                    new Rectangle(0, 0, scaled.Width, scaled.Height),
                    new Rectangle(x, y, width, height), 
                    GraphicsUnit.Pixel);
            }
            return scaled;            
        }
    }
}
