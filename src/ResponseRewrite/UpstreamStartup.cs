using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ResponseRewrite
{
    public class UpstreamStartup
    {
        public void ConfigureServices(IServiceCollection services)
        { }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(@"<!DOCTYPE html>
<html>
<body>
<ul>
    <li><a href=""http://localhost:5001/text"">Text upstream direct</a> | <a href=""text"">Text rewritten</a></li>
</ul>
</body>
</html>");
                });

                endpoints.MapGet("/text", async context =>
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
