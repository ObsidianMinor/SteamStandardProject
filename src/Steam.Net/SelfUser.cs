using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents the current user logged into the Steam client
    /// </summary>
    [DebuggerDisplay("{AccountName} : {SteamId}")]
    public class SelfUser : User
    {
        /// <summary>
        /// Get any flags on this user account
        /// </summary>
        public AccountFlags Flags { get; private set; }

        /// <summary>
        /// Gets the account name of this user. If this user is an anonymous account, the name
        /// </summary>
        public string AccountName { get; }

        /// <summary>
        /// Gets the end vanity url to access the current user's profile
        /// </summary>
        public string VanityUrl { get; private set; }

        /// <summary>
        /// Gets the email address for the current user
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets whether the current user's email is validated
        /// </summary>
        public bool EmailValidated { get; private set; }

        /// <summary>
        /// Gets whether changing credentials requires verification through email
        /// </summary>
        public bool CredentialChangeRequiresCode { get; private set; }

        /// <summary>
        /// Gets whether changing the current user's password or secret question or answer requires verification through email
        /// </summary>
        public bool PasswordOrSecretQuestionChangeRequiresCode { get; private set; }
        
        private SelfUser(SteamId id, string accountName) : base(id)
        {
            AccountName = accountName;
        }

        internal static SelfUser CreateAnonymousUser(SteamId id)
        {
            return new SelfUser(id, "anonymous");
        }

        internal static SelfUser CreateUser(SteamId id, string accountName, string vanityUrl, AccountFlags flags)
        {
            return new SelfUser(id, accountName)
            {
                VanityUrl = vanityUrl,
                Flags = flags
            };
        }

        internal void UpdateEmailInfo(string emailAddress, bool emailValidated, bool credentialChangeRequiresCode, bool passwordRequiresCode)
        {
            Email = emailAddress;
            EmailValidated = emailValidated;
            CredentialChangeRequiresCode = credentialChangeRequiresCode;
            PasswordOrSecretQuestionChangeRequiresCode = passwordRequiresCode;
        }
    }
}