using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Steam.Web
{
    public class ContentServerList
    {
        [JsonProperty("serverlist")]
        public IReadOnlyCollection<Uri> ServerList { get; set;}
    }
}