using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

#if MONOTOUCH

namespace Sqor.Utils.Drawing
{
    public partial class GraphicsPath
    {
        public void AddPolygon(PointF[] points)
        {
            var firstPoint = points.First();
            var remainingPoints = points.Skip(1);   
            
            MoveToPoint(firstPoint.X, firstPoint.Y);
            foreach (var point in remainingPoints)
            {
                AddLineToPoint(point);
            }
            AddLineToPoint(firstPoint);
            
            ClosePath();
        }
    }
}

#endif
