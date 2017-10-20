using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Steam.Web.StringSerializers
{
    public class JsonStringSerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => typeof(JToken).IsAssignableFrom(t);

        public override string ToString(object value) => (value as JToken).ToString(Formatting.None);
    }
}