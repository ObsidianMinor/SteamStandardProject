using Newtonsoft.Json;

namespace Steam.Web
{
    public class WebResponse<T>
    {
        [JsonProperty("response")] // whoever decided to put a "response" in an object should be fired
        public T Response { get; private set; }

        public static implicit operator T(WebResponse<T> response) => response.Response;
    }
}
