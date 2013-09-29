using System;

namespace Sqor.Utils.Numbers
{
    public static class NumberExtensions
    {
        public static int ToByteRatio(this int percentRatio)
        {
            var ratio = percentRatio / 100f;
            var result = (int)(ratio * 255);
            return result;
        }
        
        public static string ToAbbreviation(this int value)
        {
            if (value < 1000)
                return value.ToString();
            if (value < 1000000)
                return (value / 1000M).ToString("#.##") + "K";
            if (value < 1000000000)
                return (value / 1000000M).ToString("#.##") + "M";
            else
                return (value / 1000000000M).ToString("#.##") + "B";
        }
    }
}

