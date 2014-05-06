using System.Collections.Generic;
using System.IO;

namespace Sqor.Utils.Net
{
    public interface IHttpRequest
    {
        string Url { get; }
        string HttpMethod { get; }
        Dictionary<string, string> Headers { get; }
        Stream Input { get; }
    }
}
