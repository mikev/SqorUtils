#if MONOTOUCH

using System;
using MonoTouch.CoreGraphics;
using System.Runtime.InteropServices;
using System.Drawing;
using MonoTouch.ImageIO;
using MonoTouch.Foundation;
using Sqor.Utils.Data;
using MonoTouch.UIKit;
using System.IO;
using System.Linq;

namespace Sqor.Utils.Drawing
{
    public partial class Bitmap
    {
        private CGContext context;
        private Color fillColor;
        private Color strokeColor;
        private Font font;
        private int strokeWidth;
        private int width;
        private int height;
        private bool ownsContext;
        
        private Bitmap(CGContext context, int width, int height, bool ownsContext)
        {
            this.width = width;
            this.height = height;
            this.context = context;
            this.ownsContext = ownsContext;
        }
        
        public Bitmap(CGContext context, int width, int height) : this(context, width, height, false)
        {
        }
        
        public Bitmap(int width, int height) 
            : this(
                new CGBitmapContext(IntPtr.Zero, width, height, 8, 4 * width, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedLast),
                width, 
                height, 
                true
            )
        {
//            context.InterpolationQuality = CGInterpolationQuality.High;
//            context.SetShouldAntialias(true);
//            context.SetRGBFillColor(1, 0, 0, 1);
//            context.FillRect(new RectangleF(10, 10, 20, 20));
            
//            context.SetRGBStrokeColor(1, 0, 0, 1);
//            context.StrokeLineSegments(new[] { new PointF(0, 0), new PointF(65, 65) });
            
        }

        public static Bitmap ScaleFrom(CGImage image, Size size)
        {
            var bitmap = new Bitmap(size.Width, size.Height);
            bitmap.context.InterpolationQuality = CGInterpolationQuality.High;
            bitmap.DrawImage(image, new Rectangle(0, 0, size.Width, size.Height));            
            return bitmap;
        }
        
        public static Bitmap From(CGImage image)
        {
            var bitmap = new Bitmap(image.Width, image.Height);
            bitmap.context.InterpolationQuality = CGInterpolationQuality.High;
            bitmap.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            return bitmap;
        }
        
        public int Width
        {
            get { return width; }
        }
        
        public int Height
        {
            get { return height; }
        }
        
        public Size Size
        {
            get { return new Size(Width, Height); }
        }
        
        public Color FillColor
        {
            get { return fillColor; }
            set
            {
                fillColor = value;
                context.SetRGBFillColor(value.Red / 255f, value.Green / 255f, value.Blue / 255f, value.Alpha / 255f);
            }
        }
        
        public Color StrokeColor
        {
            get { return strokeColor; }
            set
            {
                strokeColor = value;
                context.SetRGBStrokeColor(value.Red / 255f, value.Green / 255f, value.Blue / 255f, value.Alpha / 255f);
            }
        }
        
        public int StrokeWidth
        {
            get { return strokeWidth; }
            set
            {
                strokeWidth = value;
                context.SetLineWidth(value);
            }
        }
        
        public Font Font 
        {
            get { return font; }
            set 
            {
                font = value;
                context.SelectFont(font.Name, font.Points, CGTextEncoding.MacRoman);                
            }
        }
        
        public void FillRect(Rectangle rect)
        {
            context.FillRect(rect.ToRectangleF());
        }
        
        public void StrokeRect(Rectangle rect)
        {
            context.StrokeRect(rect.ToRectangleF());
        }
        
        public void StrokeLine(params Point[] points)
        {
            context.StrokeLineSegments(points.ToPointFArray());
        }
        
        public void FillCircle(Rectangle rect)
        {
            context.FillEllipseInRect(rect);
        }
        
        public void StrokeCircle(Rectangle rect)
        {
            context.StrokeEllipseInRect(rect);
        }

        public void DrawImage(Bitmap image, Rectangle rect)
        {
            context.DrawImage(rect, image.ToImage());
        }
        
        public void DrawImage(CGImage image, Rectangle rect)
        {
            context.DrawImage(rect, image);
        }
        
        private void DrawPolygon(CGPathDrawingMode mode, PointF[] points)
        {
            var firstPoint = points.First();
            var remainingPoints = points.Skip(1);   
        
            context.MoveTo(firstPoint.X, firstPoint.Y);
            foreach (var point in remainingPoints)
            {
                context.AddLineToPoint(point.X, point.Y);
            }
        
            context.DrawPath(mode);
        }
        
        public void StrokePolygon(PointF[] points)
        {
            DrawPolygon(CGPathDrawingMode.Stroke, points);
        }
        
        public void FillPolygon(PointF[] points)
        {
            DrawPolygon(CGPathDrawingMode.Fill, points);
        }
        
        public void Translate(float x, float y)
        {
            context.TranslateCTM(x, y);
        }
        
        public void Rotate(float angle)
        {
            context.RotateCTM(angle);
        }
        
        public void Scale(float x, float y)
        {
            context.ScaleCTM(x, y);
        }
        
        public void FillPath(GraphicsPath path)
        {
            var _path = path.Realize().Item1;
            context.AddPath(_path);
            context.FillPath();
        }
        
        public void StrokePath(GraphicsPath path)
        {
            var _path = path.Realize().Item1;
            context.AddPath(_path);
            context.StrokePath();
        }
        
        public void SaveState()
        {
            context.SaveState();
        }
        
        public void RestoreState()
        {
            context.RestoreState();
        }
        
        public void DrawText(string text, Point topLeft)
        {
            context.SetTextDrawingMode(CGTextDrawingMode.Fill);
            context.ShowTextAtPoint(topLeft.X, topLeft.Y, text);
        }
        
        public void DrawTextAroundPoint(string text, Point center)
        {
            if (text == null)
                throw new ArgumentNullException("text");
        
            try
            {
                // Calculate measurement of teext
                using (var nsString = new NSString(text))
                {
                    var font = this.font.ToUIFont();    
                    var size = nsString.StringSize(font).ToSize();
                    if (text != "" && size.Width == 0 && size.Height == 0)
                        throw new InvalidOperationException("Size of non-empty string \"" + text + "\" returned an empty size.");
                    var point = new Point(center.X - size.Width / 2, center.Y - size.Height / 2);
                    DrawText(text, point);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error drawing text around point for string " + (text == null ? "null" : "\"" + text + "\"") + " at point " + center, e);
            }
        }
        
        partial void OnDispose()
        {
            if (ownsContext)
            {
                context.Dispose();
            }
        }
        
        public CGImage ToImage()
        {
            if (context is CGBitmapContext)
                return ((CGBitmapContext)context).ToImage();
            else
                throw new InvalidOperationException("Converting to an image not supported by the current CGContext");
        }
        
        public byte[] SaveToByteArray()
        {
            using (var image = ToImage())
            using (var outputData = new NSMutableData())
            using (var destination = CGImageDestination.FromData(outputData, "public.png", 1))
            {
                destination.AddImage(image, null);
                destination.Close();

                return outputData.ToByteArray();      
            }
        }
    }
}

#endif