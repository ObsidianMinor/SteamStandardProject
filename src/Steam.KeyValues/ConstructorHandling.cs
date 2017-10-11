using System;
using System.Collections.Generic;
using System.Text;

namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies how constructors are used when initializing objects during deserialization by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public enum ConstructorHandling
    {
        /// <summary>
        /// First attempt to use the public default constructor, then fall back to a single parameterized constructor, then to the non-public default constructor.
        /// </summary>
        Default = 0,

        /// <summary>
        /// KeyValue.NET will use a non-public default constructor before falling back to a parameterized constructor.
        /// </summary>
        AllowNonPublicDefaultConstructor = 1
    }
}
