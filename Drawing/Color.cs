using System;
using System.Globalization;
using Sqor.Utils.Json;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Drawing
{
    [JsonConverter(typeof(ColorJsonConverter))]
    public partial class Color
    {
        private readonly int alpha;
        private readonly int red;
        private readonly int green;
        private readonly int blue;

        public Color()
        {
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
            get { return FromRgba(255 - Red, 255 - Green, 255 - Blue); }
        }
        
        public Color HighContrast
        {
            get 
            {
                // Counting the perceptive luminance - human eye favors green color... 
                double a = 1 - ( 0.299 * Red + 0.587 * Green + 0.114 * Blue) / 255;
                int d = a < 0.5 ? 0 : 255;
    
                return FromRgba(d, d, d);            
            }
        }

        public float Hue 
        {
            get
            {
                if (Red == Green && Green == Blue)
                    return 0; // 0 makes as good an UNDEFINED value as any
      
                float r = Red / 255.0f;
                float g = Green / 255.0f;
                float b = Blue / 255.0f;

                float hue = 0.0f;
     
                float max = r; 
                float min = r;
     
                if (g > max) max = g;
                if (b > max) max = b;
     
                if (g < min) min = g;
                if (b < min) min = b;
     
                float delta = max - min;
      
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
            var max = Math.Max(Red, Math.Max(Green, Blue));
            var min = Math.Min(Red, Math.Min(Green, Blue));
        
            hue = Hue;
            saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            value = max / 255f;
        }
        
        public Color FromHsv(float hue, float saturation, float value)
        {
            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60f - (int)Math.Floor(hue / 60f);
        
            value = value * 255;
            var v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
        
            if (hi == 0)
                return FromRgba(255, v, t, p);
            else if (hi == 1)
                return FromRgba(255, q, v, p);
            else if (hi == 2)
                return FromRgba(255, p, v, t);
            else if (hi == 3)
                return FromRgba(255, p, q, v);
            else if (hi == 4)
                return FromRgba(255, t, p, v);
            else
                return FromRgba(255, v, p, q);
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
            return FromRgba(int.Parse(r, NumberStyles.HexNumber), int.Parse(g, NumberStyles.HexNumber), int.Parse(b, NumberStyles.HexNumber));
//            int rgb = Int32.Parse(color.Replace("#", ""), NumberStyles.HexNumber);
//            return Color.FromRgb(rgb);            
        }
        
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", Red, Green, Blue, Alpha);
        }

        class ColorJsonConverter : IJsonConverter
        {
            public string TypeDescription
            {
                get { return "color ('#RRGGBB')"; }
            }

            public JsonValue ToJson(object o)
            {
                return ((Color)o).ToHtmlColor();
            }

            public object FromJson(JsonValue json)
            {
                return FromHtmlColor(json);
            }
        }
    }
}
