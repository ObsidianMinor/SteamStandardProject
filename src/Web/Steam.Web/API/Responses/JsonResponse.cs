using Newtonsoft.Json;

namespace Steam.Web.API.Responses
{
    internal class JsonResponse<T>
    {
        [JsonProperty("response")]
        internal T Response { get; set; }
    }
}
