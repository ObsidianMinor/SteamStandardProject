using System;

namespace Steam.KeyValues
{
    /// <summary>
    /// A conditional in KeyValues
    /// </summary>
    [Flags]
    public enum Conditional
    {
        /// <summary>
        /// Represents no conditions
        /// </summary>
        None,
        /// <summary>
        /// A WINDOWS conditional
        /// </summary>
        Windows = 1,
        /// <summary>
        /// An OSX or macOS conditional
        /// </summary>
        MacOS = 2,
        /// <summary>
        /// A LINUX conditional
        /// </summary>
        Linux = 4,
        /// <summary>
        /// A console conditional
        /// </summary>
        Xbox360 = 8,
        /// <summary>
        /// A POSIX conditional
        /// </summary>
        Posix = MacOS | Linux,
        /// <summary>
        /// A PC conditional
        /// </summary>
        /// <remarks>Actually is WIN32</remarks>
        PC = Windows | MacOS | Linux,
        /// <summary>
        /// An inverse WINDOWS conditional
        /// </summary>
        NotWindows = Posix | Xbox360,
        /// <summary>
        /// An inverse OSX conditional
        /// </summary>
        NotMac = Windows | Linux | Xbox360,
        /// <summary>
        /// An inverse LINUX conditional
        /// </summary>
        NotLinux = Windows | MacOS | Xbox360,
        /// <summary>
        /// An inverse POSIX conditional
        /// </summary>
        NotPosix = Windows | Xbox360,
        /// <summary>
        /// An inverse PC conditional
        /// </summary>
        NotPC = Xbox360,
        /// <summary>
        /// An inverse X360 conditional
        /// </summary>
        NotXbox360 = PC,
    }
}
