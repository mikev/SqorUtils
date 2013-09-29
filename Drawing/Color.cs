using System;
using System.Globalization;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Drawing
{
    public partial class Color
    {
        public Color()
        {
        }
        
        public float Hue 
        {
            get
            {
                if (Red == Green && Green == Blue)
                    return 0; // 0 makes as good an UNDEFINED value as any
      
                float r = (float)Red / 255.0f;
                float g = (float)Green / 255.0f;
                float b = (float)Blue / 255.0f;
     
                float max, min;
                float delta;
                float hue = 0.0f;
     
                max = r; min = r;
     
                if (g > max) max = g;
                if (b > max) max = b;
     
                if (g < min) min = g;
                if (b < min) min = b;
     
                delta = max - min;
      
                if (r == max) {
                    hue = (g - b) / delta;
                }
                else if (g == max) {
                    hue = 2 + (b - r) / delta;
                }
                else if (b == max) {
                    hue = 4 + (r - g) / delta;
                }
                hue *= 60;
      
                if (hue < 0.0f) {
                    hue += 360.0f;
                }
                return hue;
            }
        }
        
        public float Saturation
        {
            get 
            {
                float hue, saturation, value;
                ToHsv(out hue, out saturation, out value);
                return saturation;
            }
        }        
        
        public float Value
        {
            get 
            {
                float hue, saturation, value;
                ToHsv(out hue, out saturation, out value);
                return value;
            }
        }        
        
        public void ToHsv(out float hue, out float saturation, out float value)
        {
            int max = Math.Max(Red, Math.Max(Green, Blue));
            int min = Math.Min(Red, Math.Min(Green, Blue));
        
            hue = Hue;
            saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            value = max / 255f;
        }
        
        public Color FromHsv(float hue, float saturation, float value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            float f = hue / 60f - (int)Math.Floor(hue / 60f);
        
            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
        
            if (hi == 0)
                return Color.FromRgba(255, v, t, p);
            else if (hi == 1)
                return Color.FromRgba(255, q, v, p);
            else if (hi == 2)
                return Color.FromRgba(255, p, v, t);
            else if (hi == 3)
                return Color.FromRgba(255, p, q, v);
            else if (hi == 4)
                return Color.FromRgba(255, t, p, v);
            else
                return Color.FromRgba(255, v, p, q);
        }
        
        public string ToHtmlColor()
        {
            return "#" + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }
        
        public static Color FromHtmlColor(string color)
        {
            if (color == null)
                return null;
                
            color = color.ChopStart("#");
            var r = color.Substring(0, 2);
            var g = color.Substring(2, 2);
            var b = color.Substring(4, 2);
            return Color.FromRgb(int.Parse(r, NumberStyles.HexNumber), int.Parse(g, NumberStyles.HexNumber), int.Parse(b, NumberStyles.HexNumber));
//            int rgb = Int32.Parse(color.Replace("#", ""), NumberStyles.HexNumber);
//            return Color.FromRgb(rgb);            
        }
        
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", Red, Green, Blue, Alpha);
        }
    }
}

