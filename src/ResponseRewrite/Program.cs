using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ResponseRewrite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var upstreamHost = Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<UpstreamStartup>()
                        .UseUrls("http://localhost:5001");
                })
                .Build();

            var proxyHost = Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<ProxyStartup>()
                        .UseUrls("http://localhost:5000");
                })
                .Build();

            upstreamHost.StartAsync();
            proxyHost.Run();
            upstreamHost.StopAsync();
        }
    }
}
