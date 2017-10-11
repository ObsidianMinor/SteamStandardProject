﻿using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Instructs the <see cref="KeyValueSerializer"/> to deserialize properties with no matching class member into the specified collection and write values during serialization
    /// </summary>
    public class KeyValueExtensionDataAttribute : Attribute
    {
        /// <summary>
        /// Instructs the <see cref="KeyValueSerializer"/> to also deserialize and serialize properties with autogenerated key names
        /// </summary>
        public bool IncludeAutogeneratedKeys { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to write extension data when serializing the object.
        /// </summary>
        public bool WriteData { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether to read extension data when deserializing the object
        /// </summary>
        public bool ReadData { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueExtensionDataAttribute"/> class.
        /// </summary>
        public KeyValueExtensionDataAttribute()
        {
            WriteData = true;
            ReadData = true;
        }
    }
}
