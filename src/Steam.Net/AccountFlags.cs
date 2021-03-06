using System;

#pragma warning disable
namespace Steam.Net
{
    /// <summary>
    /// An enum of user account flags
    /// </summary>
    [Flags]
	public enum AccountFlags
	{
		NormalUser = 0,
		PersonaNameSet = 1,
		Unbannable = 2,
		PasswordSet = 4,
		Support = 8,
		Admin = 16,
		Supervisor = 32,
		AppEditor = 64,
		HWIDSet = 128,
		PersonalQASet = 256,
		VacBeta = 512,
		Debug = 1024,
		Disabled = 2048,
		LimitedUser = 4096,
		LimitedUserForce = 8192,
		EmailValidated = 16384,
		MarketingTreatment = 32768,
		OGGInviteOptOut = 65536,
		ForcePasswordChange = 131072,
		ForceEmailVerification = 262144,
		LogonExtraSecurity = 524288,
		LogonExtraSecurityDisabled = 1048576,
		Steam2MigrationComplete = 2097152,
		NeedLogs = 4194304,
		Lockdown = 8388608,
		MasterAppEditor = 16777216,
		BannedFromWebAPI = 33554432,
		ClansOnlyFromFriends = 67108864,
		GlobalModerator = 134217728,
		ParentalSettings = 268435456,
		ThirdPartySupport = 536870912,
		NeedsSSANextSteamLogon = 1073741824,
	}
}