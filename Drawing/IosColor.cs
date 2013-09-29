#if MONOTOUCH

using System;
using MonoTouch.UIKit;

namespace Sqor.Utils.Drawing
{
    public partial class Color
    {
        private readonly int alpha;
        private readonly int red;
        private readonly int green;
        private readonly int blue;
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
        
        public static Color FromRgb(int red, int green, int blue)
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
        
        public int Alpha 
        {
            get { return alpha; }
        }
        
        public int Red
        {
            get { return red; }
        }
        
        public int Green
        {
            get { return green; }
        }
        
        public int Blue 
        {
            get { return blue; }
        }
        
        public Color Opposite
        {
            get { return FromHsv(360 - Hue, Saturation, Value); }
        }
        
        public Color Inverted
        {
            get { return FromRgb(255 - Red, 255 - Green, 255 - Blue); }
        }
        
        public static implicit operator UIColor(Color color)
        {
            return color.source;
        }
        
        public Color HighContrast
        {
            get 
            {
                int d = 0;
    
                // Counting the perceptive luminance - human eye favors green color... 
                double a = 1 - ( 0.299 * Red + 0.587 * Green + 0.114 * Blue) / 255;
    
                if (a < 0.5)
                    d = 0; // bright colors - black font
                else
                    d = 255; // dark colors - white font
    
                return FromRgb(d, d, d);            
            }
        }
    }
}

#endif