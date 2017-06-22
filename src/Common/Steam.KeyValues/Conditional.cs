namespace Steam.KeyValues
{
    /// <summary>
    /// A conditional in KeyValues
    /// </summary>
    public enum Conditional
    {
        /// <summary>
        /// A console conditional
        /// </summary>
        X360,
        /// <summary>
        /// A PC conditional
        /// </summary>
        /// <remarks>Actually is WIN32</remarks>
        PC,
        /// <summary>
        /// A Windows conditional
        /// </summary>
        WINDOWS,
        /// <summary>
        /// An OSX or macOS conditional
        /// </summary>
        OSX,
        /// <summary>
        /// A Linux conditional
        /// </summary>
        LINUX,
        /// <summary>
        /// A POSIX conditional
        /// </summary>
        POSIX
    }
}
