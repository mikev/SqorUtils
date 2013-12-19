using System.Threading.Tasks;

namespace Sqor.Utils.Net
{
    public interface IHttpAdapter
    {
        Task<IHttpResponse> Open(IHttpRequest request);
    }
}