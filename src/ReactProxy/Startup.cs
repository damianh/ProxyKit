using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ProxyKit;

namespace ReactProxy
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProxy();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.UseMiddleware<ReactHotReloadPathRewriteMiddleware>("/spa");
            app.Map("/spa", spaApp =>
            {
                spaApp.UseWebSocketProxy(context => "ws://localhost:3000");
                spaApp.RunProxy(context => 
                    context
                        .ForwardTo("http://localhost:3000")
                        .Send());
            });
            app.Run(async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}