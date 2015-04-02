using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sqor.Utils
{
    public class CurlGenerator
    {
        public async static Task<string> GenerateCurl(HttpRequestMessage request) 
        {
            var builder = new StringBuilder();

            builder.Append("curl -v -X " + request.Method + " ");

            if (request.Content != null && request.Content.Headers.ContentType != null)
                builder.Append("-H \"Content-Type: " + request.Content.Headers.ContentType + "\" ");

            foreach (var header in request.Headers)
            {
                if (header.Value != null)
                    builder.Append("-H \"" + header.Key + ": " + header.Value + "\" ");
            }

            if (request.Content is StringContent)
            {
                var stringContent = (StringContent)request.Content;
                var stringRequestData = await stringContent.ReadAsStringAsync();
                var quote = stringRequestData.Contains("'") ? "\"" : "'";
                builder.Append("--data " + quote);
                builder.Append(stringRequestData);
                builder.Append(quote);
                builder.Append(" ");
            }

            builder.Append(request.RequestUri.ToString().Replace("&", "\\&"));

            return builder.ToString();
        }
    }
}

