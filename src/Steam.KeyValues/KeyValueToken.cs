namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies a KeyValue token value type
    /// </summary>
    public enum KeyValueToken
    {
        /// <summary>
        /// A read method or write method has not been called
        /// </summary>
        None = -1,
        /// <summary>
        /// An object start value
        /// </summary>
        Start = 0,
        /// <summary>
        /// A string
        /// </summary>
        String = 1,
        /// <summary>
        /// An Int32
        /// </summary>
        Int32 = 2,
        /// <summary>
        /// A float
        /// </summary>
        Float32 = 3,
        /// <summary>
        /// A signed pointer
        /// </summary>
        Pointer = 4,
        /// <summary>
        /// A wide string
        /// </summary>
        WideString = 5,
        /// <summary>
        /// A color
        /// </summary>
        Color = 6,
        /// <summary>
        /// An unsigned Int64
        /// </summary>
        UInt64 = 7,
        /// <summary>
        /// An end token
        /// </summary>
        End = 8,
        /// <summary>
        /// A property name
        /// </summary>
        PropertyName = 9,
        /// <summary>
        /// An Int64
        /// </summary>
        Int64 = 10,
        /// <summary>
        /// A comment
        /// </summary>
        Comment = 11,
        /// <summary>
        /// A conditional
        /// </summary>
        Conditional = 12,
        /// <summary>
        /// A base statement
        /// </summary>
        Base = 13,
        /// <summary>
        /// Raw KeyValues
        /// </summary>
        Raw = 14,
    }
}
