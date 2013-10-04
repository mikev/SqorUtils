#if MONOTOUCH

using System;
using MonoTouch.Foundation;
using System.Runtime.InteropServices;

namespace Sqor.Utils.Data
{
    public static class NSDataExtensions
    {
        public static byte[] ToByteArray(this NSData data)
        {
            var result = new byte[data.Length];
            Marshal.Copy(data.Bytes, result, 0, Convert.ToInt32(result.Length));
            return result;
        }
    }
}

#endif