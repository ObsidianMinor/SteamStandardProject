using System;

namespace Steam.Web.StringSerializers
{
    public class SteamIdStringSerializer : StringSerializer
    {
        public override bool CanConvert(Type t) => t == typeof(SteamId);

        public override string ToString(object value) => ((SteamId)value).ToCommunityId().ToString();
    }
}