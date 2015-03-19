using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Sqor.Utils.Net
{
    public static class UriExtensions
    {
        public static Dictionary<string, string> DecodeQueryParameters(this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            if (uri.Query.Length == 0)
                return new Dictionary<string, string>();

            return uri.Query.TrimStart('?')
                .Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(kvp => kvp.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(kvp => kvp[0],
                    kvp => kvp.Length > 2 ? string.Join("=", kvp, 1, kvp.Length - 1) : (kvp.Length > 1 ? kvp[1] : ""));
        }

    }
}

