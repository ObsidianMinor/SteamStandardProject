using System;

namespace Steam.Web
{
    public class StringSerializer
    {
        private readonly static StringSerializer _serializer = new StringSerializer();
        internal static StringSerializer Instance => _serializer;

        public virtual bool CanConvert(Type t) => true;

        public virtual string ToString(object value) => value?.ToString() ?? "";
    }
}
