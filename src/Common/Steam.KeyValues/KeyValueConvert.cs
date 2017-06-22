using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.KeyValues
{
    public static class KeyValueConvert
    {
        internal static string ToString(this Conditional conditional)
        {
            switch (conditional)
            {
                case Conditional.X360:
                    return "$X360";
                case Conditional.WINDOWS:
                    return "$WINDOWS";
                case Conditional.POSIX:
                    return "$POSIX";
                case Conditional.PC:
                    return $"WIN32";
                case Conditional.OSX:
                    return "$OSX";
                case Conditional.LINUX:
                    return "$LINUX";
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditional));
            }
        }

        internal static bool IsNumberType(this KeyValueToken token)
        {
            return token != KeyValueToken.End && token != KeyValueToken.None && token != KeyValueToken.PropertyName;
        }

        public static T DeserializeObject<T>(string value)
        {
            throw new NotImplementedException();
        }

        public static void PopulateObject(object value, string keyValues)
        {
            throw new NotImplementedException();
        }
    }
}
