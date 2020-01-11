using System;

namespace ProxyKit.v3
{
    public class ReverseProxyOptions
    {
        public TimeSpan? WebSocketKeepAliveInterval { get; set; }

        public int? WebSocketBufferSize { get; set; }
    }
}
