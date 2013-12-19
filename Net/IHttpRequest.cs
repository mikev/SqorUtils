using System.Collections.Generic;

namespace Sqor.Utils.Net
{
    public interface IHttpRequest
    {
        string Url { get; }
        string HttpMethod { get; }
        Dictionary<string, string> Headers { get; }
        byte[] Input { get; }
    }
}