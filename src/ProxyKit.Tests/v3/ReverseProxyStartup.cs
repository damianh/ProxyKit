using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable 1998

namespace ProxyKit.v3
{
    public class ReverseProxyStartup
    {
        private readonly IConfiguration _config;

        public ReverseProxyStartup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();

            var timeout = _config.GetValue("timeout", 60);
            services.AddReverseProxy(httpClientBuilder => httpClientBuilder
                .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(timeout)));
            services.AddSingleton<ReverseProxyHandler>();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider sp)
        {
            app.UseForwardedHeadersWithPathBase();
            app.UseRouting();
            app.UseEndpoints(routes =>
            {
                routes.MapReverseProxy("/accepted/{*url}", 
                    context => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));
                routes.MapReverseProxy("/forbidden/{*url}",
                    context => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Forbidden)));

                var port = _config.GetValue("Port", 0);
                if (port != 0)
                {
                    routes.MapReverseProxy("/realserver/{*url}", context =>
                    {
                        var forwardContext = context.ForwardTo("http://localhost:" + port + "/");

                        return forwardContext
                            .AddXForwardedHeaders()
                            .Send();
                    });

                    routes.MapReverseProxy<ReverseProxyHandler>("/realserver-typedhandler/{*url}");

                    app.UseWebSockets();
                    app.Map("/ws", appInner =>
                    {
                        appInner.UseWebSocketProxy(
                            _ => new Uri($"ws://localhost:{port}/ws/"),
                            options => options.AddXForwardedHeaders());
                    });

                    app.Map("/ws-custom", appInner =>
                    {
                        appInner.UseWebSocketProxy(
                            _ => new Uri($"ws://localhost:{port}/ws-custom/"),
                            options => options.SetRequestHeader("X-TraceId", "123"));
                    });
                }
            });

            
        }

        private class ReverseProxyHandler : IReverseProxyHandler
        {
            private readonly IConfiguration _config;

            public ReverseProxyHandler(IConfiguration config)
            {
                _config = config;
            }

            public Task<HttpResponseMessage> Handle(HttpContext context)
            {
                var port = _config.GetValue("Port", 0);

                return context
                    .ForwardTo("http://localhost:" + port + "/")
                    .AddXForwardedHeaders()
                    .Send();
            }
        }
    }
}