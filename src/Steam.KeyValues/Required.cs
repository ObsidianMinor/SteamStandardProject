namespace Steam.KeyValues
{
    /// <summary>
    /// Indicating whether a property is required.
    /// </summary>
    public enum Required
    {
        /// <summary>
        /// The property is not required. The default state.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The property must be defined in KeyValue but can be a null value.
        /// </summary>
        AllowNull = 1,

        /// <summary>
        /// The property must be defined in KeyValue and cannot be a null value.
        /// </summary>
        Always = 2,

        /// <summary>
        /// The property is not required but it cannot be a null value.
        /// </summary>
        DisallowNull = 3
    }
}