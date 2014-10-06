﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Goheer.EXIF;
using ImageMagick;
using Sqor.Utils.Logging;
using Sqor.Utils.Net;


namespace Sqor.Utils.Images
{
    public enum FocusPoint : int
    {
        Objects = 32,
        TopCenter = 64,
        TopLeft = 128,
        BottomCenter = 256,
        BottomLeft = 512,
        SixthDownCenter = 1024,
        Center = 2048
    }

    public enum ScaleMode : int
    {
        None = 0,
        ContainBoth = 2,
        FitBoth = 4,
        FitWidth = 8,
        FitHeight = 16
    }

    /// <summary>
    /// Describes transformations to the images.
    /// </summary>
    [Flags]
    public enum ImageTransform
    {
        /// <summary>
        /// No transformations.
        /// </summary>
        None = 0,

        /// <summary>
        /// Apply Exif rotation.
        /// </summary>
        RotateExif = 1,

        /// <summary>
        /// Scale the height and width so that one dimension matches the specified size and the other dimension is larger than the specified size.
        /// </summary>
        ScaleContainBoth = 2,
        /// <summary>
        /// Scale the height and width so that one dimension matches the specified size and the other dimension is smaller than the specified size.
        /// </summary>
        ScaleFitBoth = 4,

        /// <summary>
        /// Scale the width of the image to match the specified width, let the height change to maintain aspect ratio.
        /// </summary>
        ScaleFitWidth = 8,

        /// <summary>
        /// Scale the height of the image to match the specified height, let the width change to maintain aspect ratio.
        /// </summary>
        ScaleFitHeight = 16,

        /// <summary>
        /// Crop the image
        /// </summary>
        CropFocusObjects = 32,

        /// <summary>
        /// Crop the image, focus on the top center. (Removing sides and lower section when necessary).
        /// </summary>
        CropFocusTopCenter = 64,

        /// <summary>
        /// Crop the image, focus on the top left. (Removing the right and bottom section when necessary).
        /// </summary>
        CropFocusTopLeft = 128,

        /// <summary>
        /// Crop the image, focus on the bottom center. (Removing the top and sides when necessary).
        /// </summary>
        CropFocusBottomCenter = 256,

        /// <summary>
        /// Crop the image, focus on the bottom left. (Removing the top and right when necessary).
        /// </summary>
        CropFocusBottomLeft = 512,

        /// <summary>
        /// Crop the image focus on the center 1/6 of the distance from the top. (Removing from the bottom, sides, and top when necessary).
        /// </summary>
        CropFocusSixthDownCenter = 1024,

        /// <summary>
        /// Crop the image focus on the center of the image, removing from all sides when necessary.
        /// </summary>
        CropFocusCenter = 2048, 

        /// <summary>
        /// If the format is png, convert it to a jpg.
        /// </summary>
        [Obsolete("Use ConvertToJpg instead.")]
        PngToJpg = 4096,

        /// <summary>
        /// Convert image to jpeg, may be ignored for animated gifs in the future.
        /// </summary>
        ConvertToJpg = 4096
    }

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
            return image.RawFormat.GetFileExtension();
        }

        public static string GetFileExtension(this ImageFormat format)
        {
            var extension = ImageCodecInfo.GetImageEncoders()
                .First(x => x.FormatID == format.Guid).FilenameExtension
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Trim('*')
                .ToLower();
            return extension;
        }

        public static string GetMimeType(this Image image)
        {
            return image.RawFormat.GetMimeType();
        }

        public static string GetMimeType(this ImageFormat imageFormat)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.First(codec => codec.FormatID == imageFormat.Guid).MimeType;
        }

        public static byte[] SaveToBytes(this Image image, ImageFormat imageFormat = null, long quality = 100L)
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
                        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
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

        /// <summary>
        /// Applies image transformations using Imagic Magick, which requires C++ redistributable to be installed on servers (http://www.microsoft.com/en-us/download/details.aspx?id=30679).
        /// </summary>
        /// <param name="origUrl"></param>
        /// <param name="transform"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="maxFileSizeInBytes"></param>
        /// <returns></returns>
        public static async Task<byte[]> TransformImage(string origUrl, ImageTransform transform, int? width = null, int? height = null, int? maxFileSizeInBytes = null)
        {
            if (string.IsNullOrWhiteSpace(origUrl))
            {
                throw new ArgumentException("Image URL must be provided.", "origUrl");
            }

            if (transform == ImageTransform.None)
            {
                throw new Exception("I can only apply transformations");
            }

            var downloadError = false;
            var bytes = await Http.To(origUrl).Get().WhenStatusIs(x => (int)x < 200 || (int)x >= 300).ReturnTrue(x => downloadError = x).AsBinary();
            if (downloadError || bytes == null || bytes.Length == 0)
            {
                throw new ArgumentException(string.Format("Unable to download image ({0})", origUrl));
            }
            using (var image = new MagickImage(bytes))
            {
                if (width == null)
                {
                    width = image.Width;
                }
                if (height == null)
                {
                    height = image.Height;
                }
                if (width <= 0 || height <= 0)
                {
                    // height and width must be greater than 0.
                    throw new ArgumentException("Height and width must be greater than 0.");
                }

                if (transform.HasFlag(ImageTransform.RotateExif))
                {
                    image.AutoOrient();
                }

                var sm = ScaleMode.None;
                if (transform.HasFlag(ImageTransform.ScaleContainBoth))
                {
                    sm = ScaleMode.ContainBoth;
                }
                else if (transform.HasFlag(ImageTransform.ScaleFitBoth))
                {
                    sm = ScaleMode.FitBoth;
                }
                else if (transform.HasFlag(ImageTransform.ScaleFitHeight))
                {
                    sm = ScaleMode.FitHeight;
                }
                else if (transform.HasFlag(ImageTransform.ScaleFitWidth))
                {
                    sm = ScaleMode.FitWidth;
                }

                ScaleImage(width.GetValueOrDefault(), height.GetValueOrDefault(), sm, image);

                if (image.Width < width)
                {
                    width = image.Width;
                }
                if (image.Height < height)
                {
                    height = image.Height;
                }

                var crop = false;
                var fp = FocusPoint.SixthDownCenter;

                if (transform.HasFlag(ImageTransform.CropFocusBottomCenter))
                {
                    crop = true;
                    fp = FocusPoint.BottomCenter;
                }
                else if (transform.HasFlag(ImageTransform.CropFocusBottomLeft))
                {
                    crop = true;
                    fp = FocusPoint.BottomLeft;
                }
                else if (transform.HasFlag(ImageTransform.CropFocusCenter))
                {
                    crop = true;
                    fp = FocusPoint.Center;
                }
                else if (transform.HasFlag(ImageTransform.CropFocusObjects))
                {
                    crop = true;
                    fp = FocusPoint.Objects;
                }
                else if (transform.HasFlag(ImageTransform.CropFocusSixthDownCenter))
                {
                    crop = true;
                    fp = FocusPoint.SixthDownCenter;
                }
                else if (transform.HasFlag(ImageTransform.CropFocusTopCenter))
                {
                    crop = true;
                    fp = FocusPoint.TopCenter;
                }
                else if (transform.HasFlag(ImageTransform.CropFocusTopLeft))
                {
                    crop = true;
                    fp = FocusPoint.TopLeft;
                }

                if (crop)
                {
                    CropImage(width, height, fp, image);
                }

                if ((image.Format == MagickFormat.Jpeg || image.Format == MagickFormat.Jpg) || (transform.HasFlag(ImageTransform.ConvertToJpg) && image.Format != MagickFormat.Gif))
                {
                    image.Format = MagickFormat.Jpeg;
                    image.Interlace = Interlace.Plane;
                }

                image.Strip();
                var result = image.ToByteArray();

                // force file size
                if (maxFileSizeInBytes.HasValue)
                {
                    while (result.Length > maxFileSizeInBytes)
                    {
                        image.Scale(new Percentage(75.0));
                        result = image.ToByteArray();
                    }
                }

                return result;
            }
        }

        public static void CropImage(int? width, int? height, FocusPoint focus, MagickImage image)
        {
            var top = 0;
            var left = 0;
            switch (focus)
            {
                case FocusPoint.TopLeft:
                    top = 0;
                    left = 0;
                    break;
                case FocusPoint.TopCenter:
                    top = 0;
                    left = (int)Math.Max(0, (image.Width / 2.0) - (width.GetValueOrDefault() / 2.0));
                    break;
                case FocusPoint.SixthDownCenter:
                    var roomLeft = image.Height - (height + image.Height / 6.0);
                    top = 0;
                    if (roomLeft > 0)
                    {
                        top = (int)(image.Height / 6.0);
                    }
                    left = (int)Math.Max(0, (image.Width / 2.0) - (width.GetValueOrDefault() / 2.0));
                    break;
                case FocusPoint.Center:
                    top = (int)Math.Max(0, (image.Height / 2.0) - (height.GetValueOrDefault() / 2.0));
                    left = (int)Math.Max(0, (image.Width / 2.0) - (width.GetValueOrDefault() / 2.0));
                    break;
                case FocusPoint.BottomLeft:
                    top = (int)Math.Max(0, image.Height - height.GetValueOrDefault());
                    left = 0;
                    break;
                case FocusPoint.BottomCenter:
                    top = (int)Math.Max(0, image.Height - height.GetValueOrDefault());
                    left = (int)Math.Max(0, (image.Width / 2.0) - (width.GetValueOrDefault() / 2.0));
                    break;
            }
            var g = new MagickGeometry(left, top, width.GetValueOrDefault(), height.GetValueOrDefault());
            image.Crop(g);
        }

        public static void ScaleImage(int width, int height, ScaleMode scale, MagickImage image)
        {
            if (scale == ScaleMode.None)
            {
                return;
            }

            int scaledHeight = height;
            int scaledWidth = width;
            var aspectRatio = ((double)image.Width) / image.Height;

            var widthScaledToHeight = (int)(aspectRatio * height);
            var heightScaledToWidth = (int)(width / aspectRatio);
            switch (scale)
            {
                case ScaleMode.ContainBoth:

                    if (widthScaledToHeight < width)
                    {
                        scaledHeight = heightScaledToWidth;
                    }
                    else if (heightScaledToWidth < height)
                    {
                        scaledWidth = widthScaledToHeight;
                    }
                    else
                    {
                        scaledHeight = heightScaledToWidth;
                    }
                    break;
                case ScaleMode.FitBoth:
                    if (widthScaledToHeight > width)
                    {
                        scaledHeight = heightScaledToWidth;
                    }
                    else if (heightScaledToWidth > height)
                    {
                        scaledWidth = widthScaledToHeight;
                    }
                    else
                    {
                        scaledHeight = heightScaledToWidth;
                    }
                    break;
                case ScaleMode.FitHeight:
                    // set the height to the max value.
                    scaledWidth = widthScaledToHeight;
                    break;
                case ScaleMode.FitWidth:
                    scaledHeight = heightScaledToWidth;
                    break;
            }
            image.Scale(scaledWidth, scaledHeight);
        }

    }
}
