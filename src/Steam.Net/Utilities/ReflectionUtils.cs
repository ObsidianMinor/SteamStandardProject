using System;
using System.Collections.Generic;

namespace Steam.Net.Utilities
{
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Gets all types the provided object inherits
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetAllTypes(this object obj)
        {
            for (Type type = obj.GetType(); type != null; type = type.BaseType)
                yield return type;
        }
    }
}
