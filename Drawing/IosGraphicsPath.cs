using System;
using MonoTouch.CoreGraphics;
using System.Drawing;
using System.Collections.Generic;

namespace Sqor.Utils.Drawing
{
    public partial class GraphicsPath
    {
        delegate void TransformHandler(ref CGAffineTransform transform);
    
        private List<Action<CGPath, CGAffineTransform>> pathActions = new List<Action<CGPath, CGAffineTransform>>();
        private List<TransformHandler> transformActions = new List<TransformHandler>();
    
        public GraphicsPath()
        {
        }
        
        public GraphicsPath Clone()
        {
            var result = new GraphicsPath();
            result.pathActions.AddRange(pathActions);
            result.transformActions.AddRange(transformActions);
            return result;
        }
        
        public Tuple<CGPath, CGAffineTransform> Realize()
        {
            var transform = CGAffineTransform.MakeIdentity();
            foreach (var action in transformActions)
                action(ref transform);
            var path = new CGPath();
            foreach (var action in pathActions)
                action(path, transform);
            return Tuple.Create(path, transform);
        }
        
        public void ClosePath()
        {
            pathActions.Add((path, transform) => path.CloseSubpath());
        }
        
        public void AddArc(float x, float y, float radius, float startAngle, float endAngle, bool clockwise)
        {
            pathActions.Add((path, transform) => path.AddArc(transform, x, y, radius, startAngle, endAngle, clockwise));
        }
        
        public void AddRelativeArc(float x, float y, float radius, float startAngle, float delta)
        {
            pathActions.Add((path, transform) => path.AddRelativeArc(transform, x, y, radius, startAngle, delta));
        }
        
        public void AddArcToPoint(float x1, float y1, float x2, float y2, float radius)
        {
            pathActions.Add((path, transform) => path.AddArcToPoint(transform, x1, y1, x2, y2, radius));
        }
        
        public void AddCurveToPoint(PointF cp1, PointF cp2, PointF point)
        {
            pathActions.Add((path, transform) => path.AddCurveToPoint(transform, cp1, cp2, point));
        }
        
        public void AddLines(params PointF[] points)
        {
            pathActions.Add((path, transform) => path.AddLines(transform, points));
        }
        
        public void AddLineToPoint(PointF point)
        {
            pathActions.Add((path, transform) => 
            {
                path.AddLineToPoint(transform, point);
            });
        }
        
        public void AddPath(GraphicsPath path2)
        {
            var realized = path2.Realize();
            pathActions.Add((path, transform) => { transform.Multiply(realized.Item2); path.AddPath(transform, realized.Item1); });
        }
        
        public void AddQuadCurveToPoint(float cpx, float cpy, float x, float y)
        {
            pathActions.Add((path, transform) => path.AddQuadCurveToPoint(transform, cpx, cpy, x, y));
        }
        
        public void AddRect(RectangleF rect)
        {
            pathActions.Add((path, transform) => path.AddRect(transform, rect));
        }
        
        public void AddRects(params RectangleF[] rects)
        {
            pathActions.Add((path, transform) => path.AddRects(transform, rects));
        }
        
        public void MoveToPoint(PointF point)
        {
            pathActions.Add((path, transform) => 
            {
                path.MoveToPoint(transform, point);
            });
        }
        
        public void MoveToPoint(float x, float y)
        {
            pathActions.Add((path, transform) => 
            {
                path.MoveToPoint(transform, x, y);
            });
        }
        
        public void AddEllipseInRect(RectangleF rect)
        {
            pathActions.Add((path, transform) => path.AddEllipseInRect(transform, rect));
        }

        public void Translate(float tx, float ty)
        {
            transformActions.Add((ref CGAffineTransform transform) => 
            {
                transform.Translate(tx, ty);
            });
        }
        
        public void Scale(float sx, float sy)
        {
            transformActions.Add((ref CGAffineTransform transform) => 
            {
                transform.Scale(sx, sy);
            });
        }
        
        public void Rotate(float angle)
        {
            transformActions.Add((ref CGAffineTransform transform) => 
            {
                transform.Rotate(angle);
            });
        }
        
        public void RotateAt(float angle, PointF pt)
        {
            float fx = pt.X;
            float fy = pt.Y;
            float fcos = (float)Math.Cos(angle);
            float fsin = (float)Math.Sin(angle);
            transformActions.Add((ref CGAffineTransform transform) =>
            {
                transform.Multiply(new CGAffineTransform(fcos, fsin, -fsin, fcos, fx - fx * fcos + fy * fsin, fy - fx * fsin - fy * fcos));
            });
        }
        
        public void Invert()
        {
            transformActions.Add((ref CGAffineTransform transform) => transform.Invert());
        }
        
        public void Multiply(GraphicsPath path)
        {
            var realized = path.Realize();
            transformActions.Add((ref CGAffineTransform transform) => transform.Multiply(realized.Item2));
        }
    }
}