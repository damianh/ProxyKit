using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProxyKit.v3;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointRouteBuilderExtensions
    {
        private const string DefaultDisplayName = "ProxyKit";
        private const string CatchAllRoutePattern = "{*url}";

        public static IEndpointConventionBuilder MapReverseProxy(
            this IEndpointRouteBuilder routes,
            HandleReverseProxyRequest handleProxyRequest) =>
            MapReverseProxy(routes, CatchAllRoutePattern, handleProxyRequest);

        public static IEndpointConventionBuilder MapReverseProxy(
            this IEndpointRouteBuilder routes,
            string pattern,
            HandleReverseProxyRequest handleReverseProxyRequest)
        {
            var pipeline = routes.CreateApplicationBuilder()
                .UseMiddleware<ReverseProxyMiddleware<DelegateReverseProxyHandler>>(
                    new DelegateReverseProxyHandler(handleReverseProxyRequest))
                .Build();

            return routes
                .Map(pattern, pipeline)
                .WithDisplayName(DefaultDisplayName);
        }

        public static IEndpointConventionBuilder MapReverseProxy<TReverseProxyHandler>(
            this IEndpointRouteBuilder routes,
            string pattern) where TReverseProxyHandler : IReverseProxyHandler
        {
            var pipeline = routes.CreateApplicationBuilder()
                .UseMiddleware<ReverseProxyMiddleware<TReverseProxyHandler>>()
                .Build();

            return routes
                .Map(pattern, pipeline)
                .WithDisplayName(DefaultDisplayName);
        }
    }
}