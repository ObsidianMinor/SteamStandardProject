namespace Steam.Common
{
    /// <summary>
    /// The Steam universes a Steam ID can exist in
    /// </summary>
    public enum Universe
    {
        /// <summary>
        /// An invalid account universe
        /// </summary>
        Invalid,
        /// <summary>
        /// The public account universe
        /// </summary>
        Public,
        /// <summary>
        /// The beta account universe
        /// </summary>
        Beta,
        /// <summary>
        /// The internal account universe
        /// </summary>
        Internal,
        /// <summary>
        /// The dev account universe
        /// </summary>
        Dev,
    }
}
