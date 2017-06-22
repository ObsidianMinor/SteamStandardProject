using System;
using System.Collections.Generic;
using System.Linq;

namespace Steam.KeyValues.Utilities
{
    internal static class EnumUtils
    {
        internal static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
