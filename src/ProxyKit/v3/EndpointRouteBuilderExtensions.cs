using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProxyKit;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointRouteBuilderExtensions
    {
        private const string DefaultDisplayName = "ProxyKit";

        public static IEndpointConventionBuilder MapReverseProxy(
            this IEndpointRouteBuilder routes,
            string pattern,
            HandleProxyRequest handleProxyRequest)
        {
            var pipeline = routes.CreateApplicationBuilder()
                .UseMiddleware<ProxyMiddleware<HandleProxyRequestWrapper>>(
                    new HandleProxyRequestWrapper(handleProxyRequest))
                .Build();

            return routes
                .Map(pattern, pipeline)
                .WithDisplayName("DefaultDisplayName");
        }

        /*
        public static void MapReverseProxy<TReverseProxy>(
            this IEndpointRouteBuilder endpoints,
            string pattern,
            Action<ReverseProxyOptions> configureOptions) where TReverseProxy : ReverseProxy
        { }
        */
    }
}