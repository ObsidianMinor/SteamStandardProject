namespace Steam
{
    /// <summary>
    /// A Steam account's account type
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// An invalid account
        /// </summary>
        Invalid,
        /// <summary>
        /// An individual account for one user
        /// </summary>
        Individual,
        /// <summary>
        /// A multiseat computer setup
        /// </summary>
        Multiseat,
        /// <summary>
        /// A registered game server
        /// </summary>
        GameServer,
        /// <summary>
        /// An anonymous game server
        /// </summary>
        AnonGameServer,
        /// <summary>
        /// A pending account
        /// </summary>
        Pending,
        /// <summary>
        /// A content server
        /// </summary>
        ContentServer,
        /// <summary>
        /// A Steam group
        /// </summary>
        Clan,
        /// <summary>
        /// A chat room between multiple users
        /// </summary>
        Chat,
        /// <summary>
        /// A console user
        /// </summary>
        ConsoleUser,
        /// <summary>
        /// An anonymous user
        /// </summary>
        AnonUser,
    }
}
