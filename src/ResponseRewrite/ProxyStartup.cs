using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyKit;

namespace ResponseRewrite
{
    public class ProxyStartup
    {
        private const string TextPlainMimeType = "text/plain";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxy();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.RunProxy(async ctx =>
            {
                var response = await ctx
                    .ForwardTo("http://localhost:5001")
                    .Send();

                if (response.Content?.Headers?.ContentType.MediaType == "text/plain")
                {
                    var rewrittenResponse = await response.ReplaceContent( async upstreamContent =>
                    {
                        // TODO Assumes the upstream response is not compressed
                        var body = await upstreamContent.ReadAsStringAsync(); // Cost: buffers the entire response
                        body = body.Replace("World", "Planet");
                        return new StringContent(body, Encoding.UTF8, "text/plain");
                    });
                    ctx.Response.RegisterForDispose(response);
                    return rewrittenResponse;
                }
                return response;
            });
        }
    }

    public delegate Task<HttpContent> RewriteContent(HttpContent upstreamContent);

    public static class HttpResponseExtensions
    {
        public static async Task<HttpResponseMessage> ReplaceContent(
            this HttpResponseMessage upstreamResponse,
            RewriteContent rewriteContent)
        {
            var response = new HttpResponseMessage();
            foreach (var header in upstreamResponse.Headers)
            {
                response.Headers.Add(header.Key, header.Value);
            }
            response.Content = await rewriteContent(upstreamResponse.Content);
            return response;
        }
    }
}