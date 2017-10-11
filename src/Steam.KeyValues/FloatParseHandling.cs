namespace Steam.KeyValues
{
    /// <summary>
    /// Specifies how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
    /// </summary>
    public enum FloatParseHandling
    {
        /// <summary>
        /// Floating point numbers are parsed to <see cref="double"/>.
        /// </summary>
        Double = 0,

        /// <summary>
        /// Floating point numbers are parsed to <see cref="decimal"/>.
        /// </summary>
        Decimal = 1
    }
}
