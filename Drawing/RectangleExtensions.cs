using System;
using System.Drawing;

namespace Sqor.Utils.Drawing
{
    public static class RectangleExtensions
    {
        public static RectangleF Grow(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left - amount, rectangle.Top - amount, rectangle.Width + amount * 2, 
                rectangle.Height + amount * 2);
        }
        
        public static RectangleF Shrink(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left + amount, rectangle.Top + amount, rectangle.Width - amount * 2, 
                rectangle.Height - amount * 2);
        }
        
        public static Rectangle Grow(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left - amount, rectangle.Top - amount, rectangle.Width + amount * 2, 
                rectangle.Height + amount * 2);
        }
        
        public static Rectangle Shrink(this Rectangle rectangle, int amount)
        {
            return rectangle.Shrink(amount, amount);
        }
        
        public static Rectangle Shrink(this Rectangle rectangle, int width, int height)
        {
            return rectangle.Shrink(width, height, width, height);
        }
        
        public static Rectangle Shrink(this Rectangle rectangle, int left, int top, int right, int bottom)
        {
            return new Rectangle(rectangle.Left + left, rectangle.Top + top, rectangle.Width - (left + right), 
                rectangle.Height - (top + bottom));
        }
        
        public static RectangleF Narrow(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width - amount, rectangle.Height);
        }
        
        public static RectangleF Widen(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width + amount, rectangle.Height);
        }
        
        public static RectangleF Shorten(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height - amount);
        }
        
        public static RectangleF Heighten(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height + amount);
        }
        
        public static RectangleF AdjustWidth(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width * amount, rectangle.Height);
        }
        
        public static RectangleF AdjustHeight(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height * amount);
        }
        
        public static RectangleF AsTallAs(this RectangleF rectangle, float amount)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, Math.Min(rectangle.Height, amount));
        }
        
        public static RectangleF OffsetX(this RectangleF rectangle, float x)
        {
            return new RectangleF(rectangle.Left + x, rectangle.Top, rectangle.Width, rectangle.Height);
        }
        
        public static RectangleF OffsetY(this RectangleF rectangle, float y)
        {
            return new RectangleF(rectangle.Left, rectangle.Top + y, rectangle.Width, rectangle.Height);
        }

        public static RectangleF ChangeHeight(this RectangleF rectangle, int height)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, height);
        }

        public static RectangleF ToRectangleF(this Rectangle rectangle)
        {
            return new RectangleF(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }
        
        public static Rectangle ToRectangle(this RectangleF rect)
        {
            return new Rectangle((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
        }
        
        public static Rectangle Narrow(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width - amount, rectangle.Height);
        }
        
        public static Rectangle Widen(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width + amount, rectangle.Height);
        }
        
        public static Rectangle Shorten(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height - amount);
        }
        
        public static Rectangle Heighten(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height + amount);
        }
        
        public static Rectangle AdjustWidth(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width * amount, rectangle.Height);
        }
        
        public static Rectangle AdjustHeight(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height * amount);
        }
        
        public static Rectangle AsTallAs(this Rectangle rectangle, int amount)
        {
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, Math.Min(rectangle.Height, amount));
        }

        public static Rectangle OffsetX(this Rectangle rectangle, int x)
        {
            return new Rectangle(rectangle.Left + x, rectangle.Top, rectangle.Width, rectangle.Height);
        }
        
        public static Rectangle OffsetY(this Rectangle rectangle, int y)
        {
            return new Rectangle(rectangle.Left, rectangle.Top + y, rectangle.Width, rectangle.Height);
        }
        
        public static Rectangle OffsetXy(this Rectangle rectangle, Point p)
        {
            return new Rectangle(rectangle.Left + p.X, rectangle.Top + p.Y, rectangle.Width, rectangle.Height);
        }
        
        public static Rectangle OffsetXy(this Rectangle rectangle, int x, int y)
        {
            return rectangle.OffsetXy(new Point(x, y));
        }
    }
}

