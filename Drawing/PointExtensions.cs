using System;
using System.Drawing;
using System.Linq;

namespace Sqor.Utils.Drawing
{
    public static class PointExtensions
    {
        public static Point[] ToPointArray(this PointF[] points)
        {
            return points.Select(x => new Point((int)x.X, (int)x.Y)).ToArray();
        }
        
        public static PointF[] ToPointFArray(this Point[] points)
        {
            return points.Select(x => new PointF(x.X, x.Y)).ToArray();
        }
        
        public static Point Offset(this Point point, Point offset)
        {
            return new Point(point.X + offset.X, point.Y + offset.Y);
        }
        
        public static PointF Offset(this PointF point, PointF offset)
        {
            return new PointF(point.X + offset.X, point.Y + offset.Y);
        }
        
        public static Point ChangeSign(this Point point)
        {
            return new Point(-point.X, -point.Y);
        }
        
        public static PointF ChangeSign(this PointF point)
        {
            return new PointF(-point.X, -point.Y);
        }
    }
}

