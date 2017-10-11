namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies metadata property handling options for the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public enum MetadataPropertyHandling
    {
        /// <summary>
        /// Read metadata properties located at the start of a KeyValue file.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Do not try to read metadata properties.
        /// </summary>
        Ignore = 2
    }
}
