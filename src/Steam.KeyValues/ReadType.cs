namespace Steam.KeyValues
{
    internal enum ReadType
    {
        Read,
        ReadAsInt32,
        ReadAsPointer, // basically ReadAsUInt32
        ReadAsInt64,
        ReadAsUInt64,
        ReadAsFloat,
        ReadAsDecimal,
        ReadAsDouble,
        ReadAsColor,
        ReadAsString,
    }
}
