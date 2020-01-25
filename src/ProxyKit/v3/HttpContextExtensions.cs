using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ProxyKit.v3
{
    public static class HttpContextExtensions
    {
        /// <summary>
        ///     Forward the request to the specified upstream host.
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <param name="upstreamHost">The upstream host to forward the request to.</param>
        /// <param name="customizeUri">An action allowing customization of the URI the request is forwarded to.</param>
        /// <returns>A <see cref="ForwardContext"/> that represents the
        /// forwarding request context.</returns>
        public static ForwardContext ForwardToV3(
            this HttpContext context,
            UpstreamHost upstreamHost,
            Action<UriBuilder>? customizeUri = default)
        {
            var catchAllUrl = context.Request.RouteValues.TryGetValue("url", out var url) 
                ? (string) url
                : "";
            var uriString = UriHelper.BuildAbsolute(
                upstreamHost.Scheme,
                upstreamHost.Host,
                upstreamHost.PathBase,
                "/" + catchAllUrl,
                context.Request.QueryString);

            Uri uri;
            if (customizeUri != null)
            {
                var uriBuilder = new UriBuilder(uriString);
                customizeUri(uriBuilder);
                if (!uriBuilder.Uri.IsAbsoluteUri)
                {
                    throw new InvalidOperationException($"{nameof(uri)} must be absolute.");
                }
                uri = uriBuilder.Uri;
            }
            else
            {
                uri = new Uri(uriString);
            }
            
            var request = context.Request.CreateProxyHttpRequest();
            request.Headers.Host = uri.Authority;
            request.RequestUri = uri;

            IHttpClientFactory httpClientFactory;
            try
            {
                httpClientFactory = context
                    .RequestServices
                    .GetRequiredService<IHttpClientFactory>();
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidOperationException(
                    $"{exception.Message} Did you forget to call services.AddProxy()?",
                    exception);
            }

            var httpClient = httpClientFactory.CreateClient(ServiceCollectionExtensions.ProxyKitHttpClientName);

            return new ForwardContext(context, request, httpClient);
        }

        private static HttpRequestMessage CreateProxyHttpRequest(this HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage();
            
            //Only copy Body when original request has a body.
            if (request.ContentLength.HasValue)
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            // Copy the request headers *except* x-forwarded-* headers.
            foreach (var header in request.Headers)
            {
                if (header.Key.StartsWith("X-Forwarded-", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            requestMessage.Method = new HttpMethod(request.Method);

            return requestMessage;
        }
    }
}
