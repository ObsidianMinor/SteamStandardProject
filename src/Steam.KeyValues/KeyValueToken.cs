namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a type of token in a KeyValue stream
    /// </summary>
    public enum KeyValueToken
    {
        /// <summary>
        /// No token
        /// </summary>
        None,
        /// <summary>
        /// A start subkeys token
        /// </summary>
        StartSubkeys,
        /// <summary>
        /// An end subkeys token
        /// </summary>
        EndSubkeys,
        /// <summary>
        /// A key token
        /// </summary>
        Key,
        /// <summary>
        /// A conditional
        /// </summary>
        Conditional,
        /// <summary>
        /// A comment
        /// </summary>
        Comment,
        /// <summary>
        /// A value token
        /// </summary>
        Value
    }
}
