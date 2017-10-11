namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies how object creation is handled by the <see cref="KeyValueSerializer"/>.
    /// </summary>
    public enum ObjectCreationHandling
    {
        /// <summary>
        /// Reuse existing objects, create new objects when needed.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Only reuse existing objects.
        /// </summary>
        Reuse = 1,

        /// <summary>
        /// Always create new objects.
        /// </summary>
        Replace = 2
    }
}
