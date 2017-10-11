namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Specified the type of token
    /// </summary>
    public enum KVTokenType
    {
        /// <summary>
        /// No token type has been set
        /// </summary>
        None,
        /// <summary>
        /// A KeyValue object
        /// </summary>
        Object,
        /// <summary>
        /// A KeyValue object property
        /// </summary>
        Property,
        /// <summary>
        /// A comment
        /// </summary>
        Comment,
        /// <summary>
        /// A string value
        /// </summary>
        String,
        /// <summary>
        /// An integer value
        /// </summary>
        Int32,
        /// <summary>
        /// A long value
        /// </summary>
        Int64,
        /// <summary>
        /// An unsigned long value
        /// </summary>
        UInt64,
        /// <summary>
        /// A float value
        /// </summary>
        Float,
        /// <summary>
        /// A pointer value
        /// </summary>
        Pointer,
        /// <summary>
        /// A color value
        /// </summary>
        Color,
        /// <summary>
        /// A raw KeyValue value
        /// </summary>
        Raw,
        /// <summary>
        /// A wide string value
        /// </summary>
        WideString
    }
}
