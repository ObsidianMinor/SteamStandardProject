using System;
using Steam.Web.Interface;

namespace Steam.Web.StringSerializers
{
    /// <summary> Converts a <see cref="Language"/> to a web API language code string </summary>
    public class LanguageSerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => t == typeof(Language);

        public override string ToString(object value) => ((Language)value).GetWebApiLanguageCode();
    }
}