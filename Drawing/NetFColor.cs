using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqor.Utils.Drawing
{
    partial class Color
    {
        private System.Drawing.Color source;

        public Color(System.Drawing.Color source)
        {
            this.source = source;
            red = source.R;
            green = source.G;
            blue = source.B;
            alpha = source.A;
        }

        public static Color FromRgba(int red, int green, int blue, int alpha = 255)
        {
            return new Color(System.Drawing.Color.FromArgb(alpha, red, green, blue));
        }

        public static implicit operator Color(System.Drawing.Color color)
        {
            return new Color(color);
        }
    }
}
