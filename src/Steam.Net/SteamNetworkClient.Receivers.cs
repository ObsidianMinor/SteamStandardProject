using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        [MessageReceiver(MessageType.ClientLogOnResponse)]
        private async Task ReceiveLogon(NetworkMessage messsage)
        {
            CMsgClientLogonResponse response = messsage.Deserialize<CMsgClientLogonResponse>();
            if (response.eresult != 1)
                await NetLog.InfoAsync($"Logon denied: {(Result)response.eresult}. Expect to disconnect").ConfigureAwait(false);

            if (response.eresult == 1)
            {
                CellId = response.cell_id;
                var id = (messsage.Header as ClientHeader).SteamId;
                InstanceId = (long)response.client_instance_id;

                await NetLog.InfoAsync($"Logged in to Steam with session Id {SessionId} and steam ID {SteamId}").ConfigureAwait(false);

                _heartbeatCancel = new CancellationTokenSource();
                _heartBeatTask = RunHeartbeatAsync(response.out_of_game_heartbeat_seconds * 1000, _heartbeatCancel.Token);
                await LoggedOn.TimedInvokeAsync(this, EventArgs.Empty, TaskTimeout, NetLog).ConfigureAwait(false);
            }
            else
            {
                await LoginRejected.InvokeAsync(this, new LoginRejectedEventArgs(response.client_supplied_steamid, (Result)response.eresult, (Result)response.eresult_extended, response.email_domain)).ConfigureAwait(false);
            }
        }

        [MessageReceiver(MessageType.ClientLoggedOff)]
        private async Task ReceiveLogOff(CMsgClientLoggedOff loggedOff)
        {
            await NetLog.InfoAsync($"Logged off: {(Result)loggedOff.eresult} ({loggedOff.eresult})").ConfigureAwait(false);
            await LoggedOff.InvokeAsync(this, new LogOffEventArgs((Result)loggedOff.eresult));
            _gracefulLogoff = true;
        }

        [MessageReceiver(MessageType.ClientWalletInfoUpdate)]
        private async Task ReceiveWallet(CMsgClientWalletInfoUpdate wallet)
        {
            var before = Wallet;
            var after = Wallet.Create((CurrencyCode)wallet.currency, wallet.balance64, wallet.balance64_delayed);
            Wallet = after;
            await WalletUpdated.InvokeAsync(this, new WalletUpdatedEventArgs(before, after)).ConfigureAwait(false);
        }
        
        [MessageReceiver(MessageType.ClientNewLoginKey)]
        private async Task ReceiveLoginKey(CMsgClientNewLoginKey newKey)
        {
            await LoginKeyReceived.InvokeAsync(this, new LoginKeyReceivedEventArgs(newKey.login_key)).ConfigureAwait(false);
            await SendAsync(NetworkMessage.CreateProtobufMessage(MessageType.ClientNewLoginKeyAccepted, new CMsgClientNewLoginKeyAccepted { unique_id = newKey.unique_id })).ConfigureAwait(false);
        }

        [MessageReceiver(MessageType.ClientServerList)]
        private Task ReceiveServerList(CMsgClientServerList list)
        {
            foreach (var server in list.servers)
            {
                var type = (ServerType)server.server_type;
                Server serverValue = new Server(server.server_ip.ToIPAddress(), (int)server.server_port);
                if (_servers.ContainsKey(type))
                    _servers[type] = _servers[type].Add(serverValue);
                else
                    _servers[type] = ImmutableHashSet.Create(serverValue);
            }

            return Task.CompletedTask;
        }

        [MessageReceiver(MessageType.ClientCMList)]
        private Task ReceiveCMList(CMsgClientCMList list)
        {
            List<Server> servers = new List<Server>(list.cm_websocket_addresses.Select(x => new Server(new Uri(x))));
            for(int i = 0; i < list.cm_addresses.Count; i++)
                servers.Add(new Server(list.cm_addresses[i].ToIPAddress(), (int)list.cm_ports[i]));

            if (_servers.ContainsKey(ServerType.ConnectionManager))
                _servers[ServerType.ConnectionManager] = _servers[ServerType.ConnectionManager].Union(servers);
            else
                _servers[ServerType.ConnectionManager] = servers.ToImmutableHashSet();

            return Task.CompletedTask;
        }

        [MessageReceiver(MessageType.ClientAccountInfo)]
        private async Task ReceivePersonaName(CMsgClientAccountInfo info)
        {
            await _friends.InitCurrentUser(info.persona_name).ConfigureAwait(false);
        }
        
        [MessageReceiver(MessageType.ClientLogOnResponse)]
        private async Task AutoLoginToFriends(CMsgClientLogonResponse response)
        {
            if (response.eresult == 1 && GetConfig<SteamNetworkConfig>().AutoLoginFriends && SteamId.FromCommunityId(response.client_supplied_steamid).IsIndividualAccount)
            {
                await NetLog.InfoAsync("Logging into friends").ConfigureAwait(false);
                await SetPersonaStateAsync(PersonaState.Online).ConfigureAwait(false);
            }
        }
        
        [MessageReceiver(MessageType.ClientFriendsList)]
        private async Task ReceiveFriendsList(CMsgClientFriendsList list)
        {
            await _friends.UpdateList(list).ConfigureAwait(false);
        }

        [MessageReceiver(MessageType.ClientPersonaState)]
        private async Task ReceivePersonaUpdate(CMsgClientPersonaState state)
        {
            await _friends.UpdateFriend(state).ConfigureAwait(false);
        }

        [MessageReceiver(MessageType.ClientClanState)]
        private async Task ReceiveClanUpdate(CMsgClientClanState state)
        {
            await _friends.UpdateClan(state);
        }

        [MessageReceiver(MessageType.ClientServersAvailable)]
        private Task ReceiveAvailableServers(CMsgClientServersAvailable available)
        {
            AuthenticationServer = (ServerType)available.server_type_for_auth_services;

            foreach(var server in available.server_types_available)
            {
                var type = (ServerType)server.server;
                var result = server.changed ? _availableServers.Remove(type) : _availableServers.Add(type);
            }

            return Task.CompletedTask;
        }
    }
}
