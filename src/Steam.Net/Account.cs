using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents a basic Steam account with a Steam ID
    /// </summary>
    [DebuggerDisplay("SteamId = {Id}")]
    public abstract class Account
    {
        /// <summary>
        /// The Steam ID of this account
        /// </summary>
        public SteamId Id { get; internal set; }

        /// <summary>
        /// Assigns the provided Steam ID to the <see cref="Id"/> property
        /// </summary>
        protected Account(SteamId id)
        {
            Id = id;
        }
    }
}
