using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents the current user logged into the Steam client
    /// </summary>
    [DebuggerDisplay("{AccountName ?? \"anonymous\"} : {Id.ToString()}")]
    public class SelfUser : User
    {
        private AccountFlags _flags;
        private string _vanityUrl;
        private string _email;
        private bool _emailValidated;

        /// <summary>
        /// Get any flags on this user account
        /// </summary>
        public AccountFlags Flags { get; }
        
        /// <summary>
        /// Gets the vanity url to access the current user's profile
        /// </summary>
        public string VanityUrl { get; }

        /// <summary>
        /// Gets the email address for the current user
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets whether the current user's email is validated
        /// </summary>
        public bool EmailValidated { get; }
        
        internal SelfUser(SteamId id) : base(id, 0)
        {
        }

        internal SelfUser WithFlags(AccountFlags flags)
        {
            var before = (SelfUser)Clone();
            before._flags = flags;
            return before;
        }

        internal SelfUser WithVanityUrl(string vanityUrl)
        {
            var before = (SelfUser)Clone();
            before._vanityUrl = vanityUrl;
            return before;
        }

        internal SelfUser WithEmail(string email)
        {
            var before = (SelfUser)Clone();
            before._email = email;
            return before;
        }

        internal SelfUser WithEmailValidation(bool validated)
        {
            var before = (SelfUser)Clone();
            before._emailValidated = validated;
            return before;
        }
    }
}