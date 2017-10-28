using System.Diagnostics;

namespace Steam.Net
{
    /// <summary>
    /// Represents a user on Steam
    /// </summary>
    [DebuggerDisplay("{PlayerName} : {SteamId}")]
    public class User : Account
    {
        public string PlayerName { get; internal set; }
        public PersonaState Status { get; internal set; }
        
        protected User(SteamId id) : base(id) { }
    }
}