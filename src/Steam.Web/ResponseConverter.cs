using Newtonsoft.Json;
using Steam.Rest;
using System;
using System.IO;

namespace Steam.Web.Interface
{
    public class ResponseConverter
    {
        internal static ResponseConverter Instance { get; } = new ResponseConverter();

        public virtual TResult ReadResponse<TResult>(RestResponse response)
        {
            using (StreamReader reader = new StreamReader(response.Content))
            {
                string data = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<TResult>(data);
            }
        }
    }
}
