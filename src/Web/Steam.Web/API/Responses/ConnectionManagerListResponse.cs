using Newtonsoft.Json;
using Steam.KeyValues;
using System.Collections.Generic;

namespace Steam.Web.API.Responses
{
    internal class ConnectionManagerListResponse : Response
    {
        [JsonProperty("serverlist")]
        [KeyValueList("serverlist")]
        internal List<string> Endpoints { get; set; }
        [JsonProperty("serverlist_websockets")]
        [KeyValueList("serverlist_websockets")]
        internal List<string> WebSocketEndpoints { get; set; }
    }
}
