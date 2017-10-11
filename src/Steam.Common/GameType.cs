namespace Steam
{
    /// <summary>
    /// Various types of games on Steam
    /// </summary>
    public enum GameType : byte
    {
        /// <summary>
        /// A Steam application
        /// </summary>
        App,
        /// <summary>
        /// A game modification
        /// </summary>
        Mod,
        /// <summary>
        /// A shortcut to a program
        /// </summary>
        Shortcut,
        /// <summary>
        /// A peer-to-peer file
        /// </summary>
        P2P
    }
}
