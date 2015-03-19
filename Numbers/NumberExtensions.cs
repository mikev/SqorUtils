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

        public static int DividePlusRemainder(this int dividend, int divisor, int valueToAddIfThereIsARemainder = 1)
        {
            int result = dividend / divisor;
            if (dividend % divisor > 0)
                result += valueToAddIfThereIsARemainder;
            return result;
        }    

        public static int ParseInt(this string s, int defaultValue = -1)
        {
            int result;
            if (!int.TryParse(s, out result))
                result = defaultValue;
            return result;
        }
        
        public static int GetProportionalValue(this int a, int c, int d)
        {
            var ratio = a / (float)c;
            var b = d * ratio;
            return (int)b;
        }
    }
}

