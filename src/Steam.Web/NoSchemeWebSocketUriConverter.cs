using System;
using Newtonsoft.Json;

namespace Steam.Web
{
    internal class NoSchemeWebSocketUriConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(Uri);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new Uri("wss://" + (string)reader.Value); // this is all your fault valve
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
