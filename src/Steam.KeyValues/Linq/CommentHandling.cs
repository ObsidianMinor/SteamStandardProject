namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Specifies how KeyValue comments are handled when loading KeyValue.
    /// </summary>
    public enum CommentHandling
    {
        /// <summary>
        /// Ignore comments.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Load comments as a <see cref="KVValue"/> with type <see cref="KVTokenType.Comment"/>.
        /// </summary>
        Load = 1
    }

    /// <summary>
    /// Specifies how line information is handled when loading KeyValue.
    /// </summary>
    public enum LineInfoHandling
    {
        /// <summary>
        /// Ignore line information.
        /// </summary>
        Ignore = 0,

        /// <summary>
        /// Load line information.
        /// </summary>
        Load = 1
    }
}
