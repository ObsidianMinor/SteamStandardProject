namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a value type in a KeyValue structure
    /// </summary>
    public enum KeyValueType
    {
        /// <summary>
        /// No value. Value consists of multiple KeyValues
        /// </summary>
        None,
        /// <summary>
        /// A string value
        /// </summary>
        String,
        /// <summary>
        /// An signed 32 bit integer value
        /// </summary>
        Int32,
        /// <summary>
        /// A 32 bit floating point value
        /// </summary>
        Float,
        /// <summary>
        /// A memory pointer value
        /// </summary>
        Pointer,
        /// <summary>
        /// A wide string value
        /// </summary>
        WideString,
        /// <summary>
        /// A RGBA color value
        /// </summary>
        Color,
        /// <summary>
        /// An unsigned 64 bit integer value
        /// </summary>
        UInt64,
        /// <summary>
        /// A signed 64 bit integer value
        /// </summary>
        Int64
    }
}
