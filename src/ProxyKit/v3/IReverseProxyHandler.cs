using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ProxyKit.v3
{
    /// <summary>
    ///     Exposes a handler which supports forwarding a request to an upstream host.
    /// </summary>
    public interface IReverseProxyHandler
    {
        /// <summary>
        ///     Represents a delegate that handles a proxy request.
        /// </summary>
        /// <param name="context">
        ///     An HttpContext that represents the incoming proxy request.
        /// </param>
        /// <returns>
        ///     A <see cref="HttpResponseMessage"/> that represents
        ///    the result of handling the proxy request.
        /// </returns>
        Task<HttpResponseMessage> Handle(HttpContext context);
    }
}