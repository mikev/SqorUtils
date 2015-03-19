using System;

namespace Sqor.Utils.Urls
{
    public class Url
    {
        public static string Combine(string baseUrl, string path)
        {
            baseUrl = baseUrl.TrimEnd('/');
            path = path.TrimStart('/');
            return baseUrl + '/' + path;
        } 
    }
}