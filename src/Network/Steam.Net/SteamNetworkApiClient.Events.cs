using Steam.Common;
using Steam.Common.Logging;
using Steam.Net.Messages.Protobufs;
using System;
using System.Collections.Generic;

namespace Steam.Net
{
    internal partial class SteamNetworkApiClient
    {
        internal event EventHandler<LogMessage> LogEvent;
        internal event EventHandler<LogonResponse> LoginActionRequestedEvent; // todo: change LogonResponse to something else
        internal event EventHandler<GameConnectTokens> GameConnectTokensReceivedEvent;
        internal event EventHandler<IReadOnlyCollection<uint>> VacStatusModifiedEvent;
        internal event EventHandler<LogonResponse> LoginRejectedEvent;
        internal event EventHandler<string> LoginKeyReceivedEvent;
        internal event EventHandler<Result> LoggedOffEvent;
        internal event EventHandler LoggedInEvent;
        internal event EventHandler<Exception> DisconnectedEvent;
        internal event EventHandler ConnectedEvent;
        internal event EventHandler CanLoginEvent;
        internal event EventHandler<MessageReceivedEventArgs> MessageReceivedEvent;
    }
}
