using System;
using System.Drawing;

#if MONOTOUCH

namespace Sqor.Utils.Drawing
{
    public static class ColorExtensions
    {
        /** 
        * Converts HSV to RGB value. 
        * 
        * @param {Integer} h Hue as a value between 0 - 360 degrees 
        * @param {Integer} s Saturation as a value between 0 - 100 % 
        * @param {Integer} v Value as a value between 0 - 100 % 
        * @returns {Array} The RGB values  EG: [r,g,b], [255,255,255] 
        */  
        public static Color FromHsv(float hue, float saturation, float value)
        {
            saturation = saturation / 100;
            value = value / 100;
  
            var hi = (int)Math.Floor((hue / 60) % 6);  
            var f = (hue / 60) - hi;  
            var p = value * (1 - saturation);  
            var q = value * (1 - f * saturation);  
            var t = value * (1 - (1 - f) * saturation);  
  
            float[] rgb;
            switch (hi) 
            {  
                case 0: 
                    rgb = new float[] { value, t, p };
                    break;  
                case 1: 
                    rgb = new float[] { q, value, p };
                    break;  
                case 2: 
                    rgb = new float[] { p, value, t };
                    break;  
                case 3: 
                    rgb = new float[] { p, q, value };
                    break;  
                case 4: 
                    rgb = new float[] { t, p, value };
                    break;  
                case 5: 
                    rgb = new float[] { value, p, q };
                    break;  
                default:
                    throw new InvalidOperationException();
            }  
  
            var r = (int)Math.Min(255, Math.Round(rgb[0] * 256));
            var g = (int)Math.Min(255, Math.Round(rgb[1] * 256));
            var b = (int)Math.Min(255, Math.Round(rgb[2] * 256));
  
            return Color.FromRgba(r, g, b);
        }
        
        /**
        * Converts RGB to HSV value.
        *
        * @param {Integer} r Red value, 0-255
        * @param {Integer} g Green value, 0-255
        * @param {Integer} b Blue value, 0-255
        * @returns {Array} The HSV values EG: [h,s,v], [0-360 degrees, 0-100%, 0-100%]
        */
        public static void ToHsv(this Color color, out int hue, out int saturation, out int value)
        {
            float r = color.Red;
            float g = color.Green;
            float b = color.Blue;
            
            r = (r / 255);
            g = (g / 255);
            b = (b / 255); 
        
            var min = Math.Min(Math.Min(r, g), b);
            var max = Math.Max(Math.Max(r, g), b);
        
            float _value = max;
            float _hue = 0;
            float _saturation;
        
            // Hue
            if (max == min) 
            {
                _hue = 0;
            } 
            else if (max == r) 
            {
                _hue = (60 * ((g - b) / (max - min))) % 360;
            } 
            else if (max == g) 
            {
                _hue = 60 * ((b - r) / (max - min)) + 120;
            } 
            else if (max == b) 
            {
                _hue = 60 * ((r - g) / (max - min)) + 240;
            }
        
            if (_hue < 0) 
            {
                _hue += 360;
            }
        
            // Saturation
            if (max == 0) 
            {
                _saturation = 0;
            } 
            else 
            {
                _saturation = 1 - (min / max);
            }
        
            hue = (int)_hue;
            saturation = (int)(_saturation * 100);
            value = (int)(_value * 100);
        }
    }
}

#endif