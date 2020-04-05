using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace ReactProxy
{
    [UsedImplicitly]
    public class ReactHotReloadPathRewriteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _spaPath;

        public ReactHotReloadPathRewriteMiddleware(RequestDelegate next, string spaPath)
        {
            _next = next;
            _spaPath = spaPath;
        }

        public async Task Invoke(HttpContext context)
        {
            // Workaround the websocket URL for reloading the SPA app is a rooted url.
            // Here we are rewriting the path so the request is handled by the SPA middleware.
            var path = context.Request.Path;
            if (path.HasValue &&
                (path.StartsWithSegments("/sockjs-node")
                 || path.StartsWithSegments("/static")
                 || path.Value.Equals("/manifest.json")
                 || path.Value.EndsWith("hot-update.json")
                 || path.Value.EndsWith("hot-update.js")))
            {
                context.Request.Path = new PathString(_spaPath + context.Request.Path);
            }
            await _next(context);
        }
    }
}