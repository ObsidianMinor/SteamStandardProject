using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Steam.Web
{
    internal class IPEndPointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(IPEndPoint);

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotSupportedException();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = ((string)reader.Value).Split(':');

            if (value.Length != 2)
                throw new InvalidOperationException();

            var address = IPAddress.Parse(value[0]);
            var port = int.Parse(value[1]);

            return new IPEndPoint(address, port);
        }
    }
}