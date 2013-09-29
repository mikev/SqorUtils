#if MONOTOUCH

using System;
using MonoTouch.UIKit;

namespace Sqor.Utils.Drawing
{
    public partial class Font
    {
        public Font(UIFont font)
        {
            name = font.Name;
            points = font.PointSize;
        }
        
        public UIFont ToUIFont()
        {
            return UIFont.FromName(name, points);
        }
        
        public Font DeriveBold()
        {
            using (var font = ToUIFont())
            {
                return new Font(font.DeriveBold());
            }
        }
    }
}

#endif