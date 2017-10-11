using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Steam.Web
{
    public class ConnectionManagerList
    {
        [JsonProperty("result")]
        public Result Result { get; private set; }
        [JsonProperty("message")]
        public string Message { get; private set; }
        [JsonProperty("serverlist")]
        public IReadOnlyCollection<IPEndPoint> ServerList { get; private set; }
        [JsonProperty("serverlist_websockets")]
        public IReadOnlyCollection<Uri> WebSocketServerList { get; private set; }
    }
}
