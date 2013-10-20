#if MONOTOUCH

using System;
using MonoTouch.UIKit;

namespace Sqor.Utils.Drawing
{
    public partial class Color
    {
        private readonly UIColor source;
        
        public Color(UIColor source)
        {
            float alpha;
            float red;
            float green;
            float blue;
            
            source.GetRGBA(out red, out green, out blue, out alpha);
            
            this.alpha = (int)(alpha * 255);
            this.red = (int)(red * 255);
            this.green = (int)(green * 255);
            this.blue = (int)(blue * 255);
            this.source = source;
        }
        
        public static implicit operator UIColor(Color color)
        {
            return color.source;
        }
        
        public static Color FromRgba(int red, int green, int blue)
        {
            return new Color(UIColor.FromRGB(red, green, blue));
        }
        
        public static Color FromRgba(int red, int green, int blue, int alpha)
        {
            return new Color(UIColor.FromRGBA(red, green, blue, alpha));
        }
        
        public static implicit operator Color(UIColor color)
        {
            return new Color(color);
        }
    }
}

#endif