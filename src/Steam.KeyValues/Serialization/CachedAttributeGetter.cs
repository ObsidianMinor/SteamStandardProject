using Steam.KeyValues.Utilities;
using System;

namespace Steam.KeyValues.Serialization
{
    internal static class CachedAttributeGetter<T> where T : Attribute
    {
        private static readonly ThreadSafeStore<object, T> TypeAttributeCache = new ThreadSafeStore<object, T>(KeyValueTypeReflector.GetAttribute<T>);

        public static T GetAttribute(object type)
        {
            return TypeAttributeCache.Get(type);
        }
    }
}
