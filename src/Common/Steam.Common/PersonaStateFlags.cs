using System;

namespace Steam.Common
{
    /// <summary>
    /// Flags a profile with specific features
    /// </summary>
    [Flags]
    public enum PersonaStateFlag
    {
        /// <summary>
        /// 
        /// </summary>
        HasRichPresence = 1,
        /// <summary>
        /// The user is in a joinable game
        /// </summary>
        InJoinableGame = 2,
        /// <summary>
        /// The user is in a web client
        /// </summary>
        ClientTypeWeb = 256,
        /// <summary>
        /// The user is in a mobile client
        /// </summary>
        ClientTypeMobile = 512,
        /// <summary>
        /// The user is in the Big Picture client
        /// </summary>
        ClientTypeTenfoot = 1024,
        /// <summary>
        /// The user is in a VR client
        /// </summary>
        ClientTypeVR = 2048,
        /// <summary>
        /// 
        /// </summary>
        LaunchTypeGamepad = 4096,
    }
}
