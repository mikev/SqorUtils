using System.Threading.Tasks;
using System.Threading;

namespace Sqor.Utils.Net
{
    public interface IHttpAdapter
    {
        Task<IHttpResponse> Open(IHttpRequest request,CancellationToken cancelToken);
    }
}