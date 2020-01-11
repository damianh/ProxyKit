using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProxyKit
{
    internal class HandleProxyRequestWrapper : IProxyHandler
    {
        private readonly HandleProxyRequest _handleProxyRequest;

        public HandleProxyRequestWrapper(HandleProxyRequest handleProxyRequest) =>
            _handleProxyRequest = handleProxyRequest;

        public Task<HttpResponseMessage> HandleProxyRequest(HttpContext httpContext)
            => _handleProxyRequest(httpContext);
    }
}