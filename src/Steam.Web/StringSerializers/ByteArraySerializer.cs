using System;
using System.Linq;

namespace Steam.Web.StringSerializers
{
    public class ByteArraySerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => t == typeof(byte[]);

        public override string ToString(object value) => string.Join("", (value as byte[]).Select(b => b.ToString("X2")));
    }
}