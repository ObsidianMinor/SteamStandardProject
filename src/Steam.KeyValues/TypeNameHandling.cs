using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies type name handling options for the <see cref="KeyValueSerializer"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="KeyValueSerializer.TypeNameHandling"/> should be used with caution when your application deserializes JSON from an external source.
    /// Incoming types should be validated with a custom <see cref="KeyValueSerializer.SerializationBinder"/>
    /// when deserializing with a value other than <see cref="None"/>.
    /// </remarks>
    [Flags]
    public enum TypeNameHandling
    {
        /// <summary>
        /// Do not include the .NET type name when serializing types.
        /// </summary>
        None = 0,

        /// <summary>
        /// Include the .NET type name when serializing into a KeyValue object structure.
        /// </summary>
        Objects = 1,

        /// <summary>
        /// Include the .NET type name when the type of the object being serialized is not the same as its declared type.
        /// Note that this doesn't include the root serialized object by default. To include the root object's type name in KeyValue
        /// you must specify a root type object with <see cref="KeyValueConvert.SerializeObject(object, Type, KeyValueSerializerSettings)"/>
        /// or <see cref="KeyValueSerializer.Serialize(KeyValueWriter, object, Type)"/>.
        /// </summary>
        Auto = 2
    }
}
