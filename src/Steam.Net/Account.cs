using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents a basic Steam account with a Steam ID
    /// </summary>
    [DebuggerDisplay("SteamId = {Id}")]
    public abstract class Account
    {
        private SteamId _id;

        /// <summary>
        /// The Steam ID of this account
        /// </summary>
        public SteamId Id => Id;

        /// <summary>
        /// Assigns the provided Steam ID to the <see cref="Id"/> property
        /// </summary>
        protected Account(SteamId id)
        {
            _id = id;
        }

        private protected virtual Account Clone()
        {
            return (Account)MemberwiseClone();
        }

        internal Account WithSteamId(SteamId id)
        {
            Account newAccount = Clone();
            newAccount._id = id;
            return newAccount;
        }
    }
}
