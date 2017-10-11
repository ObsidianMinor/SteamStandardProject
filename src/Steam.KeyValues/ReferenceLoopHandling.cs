namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies reference loop handling options for the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public enum ReferenceLoopHandling
    {
        /// <summary>
        /// Throw a <see cref="KeyValueSerializationException"/> when a loop is encountered.
        /// </summary>
        Error = 0,

        /// <summary>
        /// Ignore loop references and do not serialize.
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// Serialize loop references.
        /// </summary>
        Serialize = 2
    }
}
