using System;
using System.Drawing;

namespace Sqor.Utils.Drawing
{
    public static class SizeExtensions
    {
        public static Size Min(this Size size, int minWidth, int minHeight)
        {
            return new Size(Math.Min(size.Width, minWidth), Math.Min(size.Height, minHeight));
        }
        
        public static Size Max(this Size size, int minWidth, int minHeight)
        {
            return new Size(Math.Min(size.Width, minWidth), Math.Max(size.Height, minHeight));
        }
        
        public static Size Scale(this Size size, float ratio)
        {
            return new Size((int)(size.Width * ratio), (int)(size.Height * ratio));
        }
        
        public static SizeF Scale(this SizeF size, float ratio)
        {
            return new SizeF(size.Width * ratio, size.Height * ratio);
        }
        
        public static Size Shrink(this Size size, int shrinkWidth, int shrinkHeight)
        {
            return new Size(size.Width - shrinkWidth, size.Height - shrinkHeight);
        }
        
        public static Size ToSize(this SizeF size)
        {
            return new Size((int)size.Width, (int)size.Height);
        }
        
        public static SizeF ToSizeF(this Size size)
        {
            return new SizeF(size.Width, size.Height);
        }
    }
}

