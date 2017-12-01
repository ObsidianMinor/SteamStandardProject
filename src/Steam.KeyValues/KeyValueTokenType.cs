namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a type of token in a KeyValue stream
    /// </summary>
    public enum KeyValueTokenType
    {
        None,
        StartSubkeys,
        EndSubkeys,
        PropertyName,
        Conditional,
        Comment,
        Value
    }
}
