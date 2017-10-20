using System;

namespace Steam.Web.StringSerializers
{
    public class TimeSpanSerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => t == typeof(TimeSpan);

        public override string ToString(object value) => Math.Floor(((TimeSpan)value).TotalSeconds).ToString();
    }
}