using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ProxyKit.RoutingHandler;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ProxyKit.v3
{
    public class EndpointTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EndpointTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        [Fact]
        public async Task Proxy_via_endpoint()
        {
            var routingHandler = new RoutingMessageHandler();

            var proxyWebHostBuilder = new WebHostBuilder()
                .UseStartup<V3ProxyStartup>();
            using var proxyTestServer = new TestServer(proxyWebHostBuilder);
            routingHandler.AddHandler("http://proxy", proxyTestServer.CreateHandler());

            var upstreamWebHostBuilder = new WebHostBuilder()
                .UseStartup<V3UpstreamStartup>();
            using var upstreamTestServer = new TestServer(upstreamWebHostBuilder);
            routingHandler.AddHandler("http://upstream", upstreamTestServer.CreateHandler());

            var client = new HttpClient(routingHandler);

            var response = await client.GetAsync("http://proxy/foo/test");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            var body = await response.Content.ReadAsStringAsync();
            _outputHelper.WriteLine(body);
        }

        public class V3ProxyStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRouting();
                services.AddReverseProxy();
            }

            public void Configure(IApplicationBuilder app)
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet(
                        "blah/{*url}",
                        async ctx =>
                        {
                            var routeValues = ctx.Request.RouteValues;
                            await ctx.Response.WriteAsync(ctx.Request.GetDisplayUrl());
                        });
                    endpoints.MapReverseProxy(
                        "{service}/{*url}",
                        async ctx =>
                        {
                            var response = await ctx
                                .ForwardTo("http://upstream")
                                .Send();
                            return response;
                        });
                });
            }
        }

        public class V3UpstreamStartup
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddRouting();
            }

            public void Configure(IApplicationBuilder app)
            {
                var forwardedOptions = new ForwardedHeadersWithPathBaseOptions
                {
                    ForwardedHeaders = ForwardedHeadersWithPathBase.All
                };
                app.UseForwardedHeadersWithPathBase(forwardedOptions);
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet(
                        "{*url}", 
                        async ctx => 
                            await ctx.Response.WriteAsync(ctx.Request.GetDisplayUrl()));
                });
            }
        }
    }
}
