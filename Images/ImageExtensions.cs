using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Goheer.EXIF;
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
        CropFocusCenter = 2048
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

        public static async Task<byte[]> TransformImage(string origUrl, ImageTransform transform, int? width = null, int? height = null)
        {
            if (string.IsNullOrWhiteSpace(origUrl))
            {
                throw new ArgumentException("Image URL must be provided.", "origUrl");
            }

            var bytes = await Http.To(origUrl).Get().AsBinary();
            var image = Image.FromStream(new MemoryStream(bytes));
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
            var format = image.RawFormat;

            if (transform == ImageTransform.None)
                throw new Exception("I can only apply transformations.");

            if (transform.HasFlag(ImageTransform.RotateExif))
            {
                ImageExtensions.ExifRotate(bytes, ref image);
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

            image = ImageExtensions.ScaleImage(width.GetValueOrDefault(), height.GetValueOrDefault(), sm, image);

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
                image = ImageExtensions.CropImage(width, height, fp, image);

            var modifiedBytes = image.SaveToBytes(format);
            image.Dispose();
            return modifiedBytes;
        }

        public static RotateFlipType GetExifRotateFlip(byte[] bytes)
        {
            var bmp = new Bitmap(new MemoryStream(bytes));
            var exif = new EXIFextractor(ref bmp, "n");
            var flip = RotateFlipType.RotateNoneFlipNone;
            if (exif["Orientation"] != null)
            {
                flip = OrientationToFlipType(exif["Orientation"].ToString());
            }
            return flip;
        }

        /// <summary>
        /// Rotates the image based on the Exif data.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="image"></param>
        public static void ExifRotate(byte[] bytes, ref Image image)
        {
            var bmp = new Bitmap(new MemoryStream(bytes));
            var exif = new EXIFextractor(ref bmp, "n");
            if (exif["Orientation"] != null)
            {
                RotateFlipType flip = OrientationToFlipType(exif["Orientation"].ToString());

                if (flip != RotateFlipType.RotateNoneFlipNone)
                {
                    image.RotateFlip(flip);
                }
            }
            bmp.Dispose();
        }

        private static RotateFlipType OrientationToFlipType(string orientation)
        {
            var rv = RotateFlipType.RotateNoneFlipNone;

            switch (int.Parse(orientation))
            {
                case 1:
                    rv = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    rv = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    rv = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    rv = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    rv = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    rv = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    rv = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    rv = RotateFlipType.Rotate270FlipNone;
                    break;
            }
            return rv;
        }

        public static Image CropImage(int? width, int? height, FocusPoint focus, Image image)
        {
            var top = 0;
            var left = 0;
            if (focus == FocusPoint.Objects)
            {
                var res = GetObjectFocusPoint(ref image);
                if (res == null || !res.Any())
                {
                    // if no objects found, default to:
                    focus = FocusPoint.SixthDownCenter;
                }
                else
                {
                    var subject = res.First();
                    // TODO: pick a better focal point.. the biggest object or get the most in the viewport 

                    // try to center around center of subject
                    top = (int)Math.Max(0, (subject.Top + subject.Height / 2.0) - (height.GetValueOrDefault() / 2.0));
                    left = (int)Math.Max(0, (subject.Left + subject.Width / 2.0) - (width.GetValueOrDefault() / 2.0));
                }
            }
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
            image = image.Crop(left, top, width.GetValueOrDefault(), height.GetValueOrDefault(), width.GetValueOrDefault(),
                height.GetValueOrDefault());
            return image;
        }

        public static Image ScaleImage(int width, int height, ScaleMode scale, Image image)
        {
            int scaledHeight = height;
            int scaledWidth = width;
            if (scale != ScaleMode.None)
            {
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
                image = image.Scale(scaledWidth, scaledHeight);
            }
            return image;
        }

        public static Rectangle[] GetObjectFocusPoint(ref Image img)
        {
            if (img == null)
            {
                throw new ArgumentNullException("img", "Image must not be null.");
            }
            // try to detect faces
            /*
            var filePath = HostingEnvironment.MapPath(@"~\App_data\haarcascade_eye.xml");
            if (filePath == null)
                throw new Exception("haarcascade_eye.xml not found.");
            var xml = File.ReadAllText(filePath);
            */
            /*
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(HaarCascadeClassifer.My.Resources.Resources.haarcascade_frontalface_alt);
            //xmlDoc.LoadXml(xml);
            var detector = new HaarDetector(xmlDoc);
            int maxDetCount = int.MaxValue;
            int minNRectCount = 0;
            float firstScale = detector.Size2Scale(10);
            float maxScale = detector.Size2Scale(400);
            float scaleMult = 1.1f;
            float sizeMultForNesRectCon = 0.3f;
            float slidingRatio = 0.2f;
            Pen pen = null;
#if DEBUG
            // for debugging purposes outline object.
            pen = new Pen(Brushes.Red, 4);
#endif

            var detectorParameters = new HaarDetector.DetectionParams(maxDetCount, minNRectCount,
                firstScale, maxScale, scaleMult, sizeMultForNesRectCon, slidingRatio, pen);
            var bmp = new Bitmap(img);
            var res = detector.Detect(ref bmp, detectorParameters);

#if DEBUG
            // for debugging purposes outline object.
            img = bmp;
#endif
            return res.DetectedOLocs;
             */
            return null;
        }
    }
}
