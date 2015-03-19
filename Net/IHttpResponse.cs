using System.Collections.Generic;

namespace Sqor.Utils.Net
{
    public interface IHttpResponse
    {
        IDictionary<string, string> Headers { get; }
        int Status { get; }
        string StatusMessage { get; }
        byte[] Output { get; }
    }
}
