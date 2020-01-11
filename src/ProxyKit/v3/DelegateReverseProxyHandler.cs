using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProxyKit.v3
{
    internal class DelegateReverseProxyHandler : IReverseProxyHandler
    {
        private readonly HandleReverseProxyRequest _handleReverseProxyRequest;

        public DelegateReverseProxyHandler(HandleReverseProxyRequest handleReverseProxyRequest) =>
            _handleReverseProxyRequest = handleReverseProxyRequest;

        public Task<HttpResponseMessage> Handle(HttpContext httpContext)
            => _handleReverseProxyRequest(httpContext);
    }
}