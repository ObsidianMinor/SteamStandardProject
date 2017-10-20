using System;

namespace Steam.Net
{
	/// <summary>
	/// Specifies what information to request of a user's persona
	/// </summary>
    [Flags]
    public enum PersonaInfoFlag
    {
		/// <summary>
		/// Requests the persona state
		/// </summary>
        Status = 1,
		/// <summary>
		/// Requests the persona name
		/// </summary>
		PlayerName = 2,
		/// <summary>
		/// Requests the query port of the game the current user is playing
		/// </summary>
		QueryPort = 4,
		/// <summary>
		/// Requests the 
		/// </summary>
		SourceID = 8,
		/// <summary>
		/// 
		/// </summary>
		Presence = 16,
		/// <summary>
		/// 
		/// </summary>
		LastSeen = 64,
		/// <summary>
		/// 
		/// </summary>
		ClanInfo = 128,
		/// <summary>
		/// 
		/// </summary>
		GameExtraInfo = 256,
		/// <summary>
		/// 
		/// </summary>
		GameDataBlob = 512,
		/// <summary>
		/// 
		/// </summary>
		ClanTag = 1024,
		/// <summary>
		/// 
		/// </summary>
		Facebook = 2048,
        /// <summary> Represents all persona info </summary>
        All = Status | PlayerName | QueryPort | SourceID | Presence | LastSeen | ClanInfo | GameExtraInfo | GameDataBlob | ClanTag | Facebook
    }
}