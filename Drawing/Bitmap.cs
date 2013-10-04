using System;
using System.Drawing;

#if MONOTOUCH

namespace Sqor.Utils.Drawing
{
    public partial class Bitmap : IDisposable
    {
        partial void OnDispose();
        
        private bool isDisposing;
        private bool isDisposed;
    
        public void FillCircle(Point midpoint, int radius)
        {
            var rect = new Rectangle(midpoint.X - radius, midpoint.Y - radius, radius * 2, radius * 2);
            FillCircle(rect);
        }
        
        public void StrokeCircle(Point midpoint, int radius)
        {
            var rect = new Rectangle(midpoint.X - radius, midpoint.Y - radius, radius * 2, radius * 2);
            StrokeCircle(rect);            
        }
        
        public void FillRect(float x, float y, float width, float height)
        {
            context.FillRect(new RectangleF(x, y, width, height));
        }
        
        public void Dispose()
        {
            if (!isDisposing && !isDisposed)
            {
                isDisposing = true;
                OnDispose();
                isDisposed = true;
            }
        }
    }
}

#endif