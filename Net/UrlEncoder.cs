using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Sqor.Utils.Strings;

namespace Sqor.Utils.Net
{
    public class UrlEncoder
    {

        public static string UrlEncode(string value)
        {
            const string ReservedChars = @"`!@#$%^&*()_-+=.~,:;'?/|\[] ";
            const string UnReservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

            var result = new StringBuilder();

            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            foreach (string symbol in value.AsCodePoints())
            {
                if (symbol.Length == 1 && UnReservedChars.IndexOf(symbol, StringComparison.CurrentCulture) != -1)
                {
                    result.Append(symbol);
                }
                else if (symbol.Length == 1 && ReservedChars.IndexOf(symbol, StringComparison.CurrentCulture) != -1)
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol.ToCharArray().First()).ToUpper());
                }
                else
                {
                    string symbolString = symbol;
                    var encoded = Uri.EscapeDataString(symbolString).ToUpper();

                    if (!string.IsNullOrWhiteSpace(encoded))
                    {
                        result.Append(encoded);
                    }
                }
            }

            return result.ToString();
        }
    }
}