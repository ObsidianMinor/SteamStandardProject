using Newtonsoft.Json;
using Steam.Common;
using Steam.KeyValues;

namespace Steam.Web.API.Responses
{
    [KeyValueRoot(RootName = "response")]
    internal abstract class Response
    {
        [JsonProperty("result")]
        [KeyValueProperty("result")]
        internal Result ResultCode { get; } = Result.OK;
        [JsonProperty("message")]
        [KeyValueProperty("message")]
        internal string Message { get; }
    }
}
