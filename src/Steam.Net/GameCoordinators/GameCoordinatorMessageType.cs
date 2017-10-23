namespace Steam.Net.GameCoordinators
{
    public enum GameCoordinatorMessageType
    {
        SystemMessage = 4001,
        ReplicateConVars,
        ConVarUpdated,
        ClientWelcome,
        ServerWelcome,
        ClientHello,
        ServerHello,
        ClientConnectionStatus = 4009,
        ServerConnectionStatus,
        InviteToParty = 4501,
        InvitationCreated,
        PartyInviteResponse,
        KickFromParty,
        LeaveParty,
        ServerAvailable,
        ClientConnectToServer,
        GameServerInfo,
        Error,
        LANServerAvailable = 4511,
    }
}
