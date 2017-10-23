namespace Steam.KeyValues
{
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
