using Steam.Common;
using System.Collections.Generic;

namespace Steam.Net
{
    /// <summary>
    /// Represents the current user on the Steam client
    /// </summary>
    public class SelfUser : User
    {
        /// <summary>
        /// Gets all the licenses the current user owns
        /// </summary>
        public IReadOnlyCollection<License> Licenses { get; internal set; }

        /// <summary>
        /// Gets the last recorded persona name used by this account
        /// </summary>
        public string PersonaName { get; internal set; }
        /// <summary>
        /// Gets the country this account is conencted from
        /// </summary>
        public string Country { get; internal set; }
        /// <summary>
        /// Gets the total number of computers this account has authorized
        /// </summary>
        public int AuthedComputers { get; internal set; }
        /// <summary>
        /// Gets the account flags for this account
        /// </summary>
        public AccountFlags Flags { get; internal set; }

        public bool NotifyNewSteamGuardMachines { get; internal set; }
        
        private SelfUser(SteamNetworkClient client) : base(client)
        {
        }

        internal static SelfUser Create(SteamNetworkClient client, Universe defaultUniverse)
        {
            return new SelfUser(client)
            {
                Id = new SteamId(0, defaultUniverse, AccountType.Individual, 1)
            };
        }
    }
}
