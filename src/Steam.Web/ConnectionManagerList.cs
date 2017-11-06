using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Steam.Web
{
    public class ConnectionManagerList
    {
        [JsonProperty("serverlist", ItemConverterType = typeof(IPEndPointConverter))]
        public IReadOnlyCollection<IPEndPoint> ServerList { get; set; }
        [JsonProperty("serverlist_websockets", ItemConverterType = typeof(NoSchemeWebSocketUriConverter))]
        public IReadOnlyCollection<Uri> WebSocketServerList { get; set; }
    }
}
