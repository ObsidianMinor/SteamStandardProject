using System;

namespace Steam.Web.StringSerializers
{
    public class DateTimeSerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => t == typeof(DateTime) || t == typeof(DateTimeOffset);

        public override string ToString(object value)
        {
            switch(value)
            {
                case DateTime time:
                    DateTimeOffset offset = time;
                    return offset.ToUnixTimeSeconds().ToString();
                case DateTimeOffset realOffset:
                    return realOffset.ToUnixTimeSeconds().ToString();
                default:
                    throw new ArgumentException("Provided object is not a DateTime or DateTimeOffset", nameof(value));
            }
        }
    }
}