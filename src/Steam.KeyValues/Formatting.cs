namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies formatting options for the <see cref="KeyValueTextWriter"/>.
    /// </summary>
    public enum Formatting
    {
        /// <summary>
        /// No special formatting is applied. This is the default.
        /// </summary>
        None = 0,

        /// <summary>
        /// Causes child objects to be indented according to the <see cref="KeyValueTextWriter.Indentation"/> and <see cref="KeyValueTextWriter.IndentChar"/> settings.
        /// </summary>
        Indented = 1
    }
}
