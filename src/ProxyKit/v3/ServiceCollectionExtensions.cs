using System;
using System.Net.Http;
using ProxyKit.v3;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        internal const string ProxyKitHttpClientName = "ProxyKitClient";

        public static IServiceCollection AddReverseProxy(
            this IServiceCollection services,
            Action<IHttpClientBuilder>? configureHttpClientBuilder = null,
            Action<ReverseProxyOptions>? configureOptions = null)
        {
            var httpClientBuilder = services
                .AddHttpClient(ProxyKitHttpClientName)
                .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    UseCookies = false
                });

            configureHttpClientBuilder?.Invoke(httpClientBuilder);

            configureOptions ??= _ => { };
            services
                .Configure(configureOptions)
                .AddOptions<ReverseProxyOptions>();
            return services;
        }
    }
}