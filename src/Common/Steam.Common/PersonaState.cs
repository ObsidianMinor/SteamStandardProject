namespace Steam.Common
{
    /// <summary>
    /// Specifies persona states for a Steam user
    /// </summary>
    public enum PersonaState
    {
        /// <summary>
        /// Offline state
        /// </summary>
        Offline = 0,
        /// <summary>
        /// Online state
        /// </summary>
        Online = 1,
        /// <summary>
        /// Busy state
        /// </summary>
        Busy = 2,
        /// <summary>
        /// Away state
        /// </summary>
        Away = 3,
        /// <summary>
        /// Snooze state
        /// </summary>
        Snooze = 4,
        /// <summary>
        /// Looking for trade
        /// </summary>
        LookingToTrade = 5,
        /// <summary>
        /// Looking to play
        /// </summary>
        LookingToPlay = 6,
    }
}
