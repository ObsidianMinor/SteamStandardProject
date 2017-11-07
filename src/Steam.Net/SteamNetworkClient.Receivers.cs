using Steam.Net.GameCoordinators;
using Steam.Net.GameCoordinators.Messages;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Messages.Structs;
using Steam.Net.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        [MessageReceiver(MessageType.Multi)]
        private async Task ReceiveMulti(CMsgMulti multi)
        {
            byte[] payload = multi.message_body;
            if (multi.size_unzipped > 0)
            {
                using (MemoryStream compressedStream = new MemoryStream(payload))
                using (GZipStream decompressionStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (MemoryStream decompressedStream = new MemoryStream())
                {
                    await decompressionStream.CopyToAsync(decompressedStream).ConfigureAwait(false);
                    payload = decompressedStream.ToArray();
                }
            }

            using (MemoryStream stream = new MemoryStream(payload))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Length - stream.Position != 0)
                {
                    int subSize = reader.ReadInt32();
                    byte[] subData = reader.ReadBytes(subSize);

                    await DispatchData(subData).ConfigureAwait(false);
                }
            }
        }

        [MessageReceiver(MessageType.ChannelEncryptRequest)]
        private async Task ReceiveEncryptRequest(ChannelEncryptRequest encryptRequest)
        {
            await NetLog.VerboseAsync($"Encrypting channel on protocol version {encryptRequest.ProtocolVersion} in universe {encryptRequest.Universe}").ConfigureAwait(false);
            CurrentUser.Id = SteamId.CreateAnonymousUser(encryptRequest.Universe);

            byte[] challange = encryptRequest.Challenge.All(b => b == 0) ? encryptRequest.Challenge : null; // check if all the values were made 0 by the marshal
            byte[] publicKey = UniverseUtils.GetPublicKey(encryptRequest.Universe);
            if (publicKey == null)
            {
                await NetLog.ErrorAsync($"Cannot find public key for universe {encryptRequest.Universe}").ConfigureAwait(false);
                throw new InvalidOperationException($"Public key does not exist for universe {encryptRequest.Universe}");
            }

            byte[] tempSessionKey = CryptoUtils.GenerateBytes(32);
            byte[] encryptedHandshake = null;

            using (RsaCrypto rsa = new RsaCrypto(publicKey))
            {
                if (challange != null)
                {
                    byte[] handshakeToEncrypt = new byte[tempSessionKey.Length + challange.Length];
                    Array.Copy(tempSessionKey, handshakeToEncrypt, tempSessionKey.Length);
                    Array.Copy(challange, 0, handshakeToEncrypt, tempSessionKey.Length, challange.Length);

                    encryptedHandshake = rsa.Encrypt(handshakeToEncrypt);
                }
                else
                {
                    encryptedHandshake = rsa.Encrypt(tempSessionKey);
                }
            }
            
            Encryption = challange != null ? (IEncryptor)new HmacEncryptor(tempSessionKey) : new SimpleEncryptor(tempSessionKey);

            var encryptResponse = NetworkMessage.CreateMessage(MessageType.ChannelEncryptResponse, new ChannelEncryptResponse 
            { 
                KeySize = 128,
                KeyHash =  CryptoUtils.CrcHash(encryptedHandshake),
                EncryptedHandshake = encryptedHandshake,
                ProtocolVersion = 1,
            });
            await SendAsync(encryptResponse).ConfigureAwait(false);
        }

        [MessageReceiver(MessageType.ChannelEncryptResult)]
        private async Task ReceiveEncryptResult(ChannelEncryptResult encryptResult)
        {
            if (encryptResult.Result == Result.OK)
            {
                await NetLog.DebugAsync("Channel encrypted").ConfigureAwait(false);
                await _connection.CompleteAsync().ConfigureAwait(false);
            }
        }

        [MessageReceiver(MessageType.ClientLogOnResponse)]
        private async Task ReceiveLogon(NetworkMessage messsage)
        {
            CMsgClientLogonResponse response = messsage.Deserialize<CMsgClientLogonResponse>();
            if (response.eresult != 1)
                await NetLog.InfoAsync($"Logon denied: {(Result)response.eresult}. Expect to disconnect").ConfigureAwait(false);

            if (response.eresult == 1)
            {
                CellId = response.cell_id;
                SessionId = (messsage.Header as ClientHeader).SessionId;
                var id = (messsage.Header as ClientHeader).SteamId;
                InstanceId = (long)response.client_instance_id;
                if (id.IsAnonymousAccount || id.IsGameServer)
                    CurrentUser.Id = id;
                else
                {
                    CurrentUser.Id = id;
                    CurrentUser.Flags = (AccountFlags)response.account_flags;
                }

                await NetLog.InfoAsync($"Logged in to Steam with session Id {SessionId} and steam ID {SteamId}").ConfigureAwait(false);

                _heartbeatCancel = new CancellationTokenSource();
                _heartBeatTask = RunHeartbeatAsync(response.out_of_game_heartbeat_seconds * 1000, _heartbeatCancel.Token);
                await TimedInvokeAsync(LoggedOn, EventArgs.Empty).ConfigureAwait(false);
            }
            else
            {
                await LoginRejected.InvokeAsync(this, new LoginRejectedEventArgs(response.client_supplied_steamid, (Result)response.eresult, (Result)response.eresult_extended, response.email_domain)).ConfigureAwait(false);
            }
        }

        [MessageReceiver(MessageType.JobHeartbeat)]
        private async Task HeartbeatJob(Header header)
        {
            await _jobs.HeartbeatJob(header.JobId).ConfigureAwait(false);
        }

        [MessageReceiver(MessageType.DestJobFailed)]
        private async Task FailJob(Header header)
        {
            await _jobs.SetJobFail(header.JobId, new DestinationJobFailedException(header.JobId)).ConfigureAwait(false);
        }

        [MessageReceiver(MessageType.ClientLoggedOff)]
        private async Task ReceiveLogOff(CMsgClientLoggedOff loggedOff)
        {
            await NetLog.InfoAsync($"Logged off: {(Result)loggedOff.eresult} ({loggedOff.eresult})").ConfigureAwait(false);
            await LoggedOff.InvokeAsync(this, new LogOffEventArgs((Result)loggedOff.eresult));
            _gracefulLogoff = true;
            CurrentUser.Reset();
        }

        [MessageReceiver(MessageType.ClientFromGC)]
        private async Task RouteGCMessage(CMsgGCClient msg)
        {
            GameCoordinator gc = _gameCoordinators[(int)msg.appid];
            (var type, var protobuf) = MessageTypeUtils.SplitUInt32Message(msg.msgtype);

            await gc.DispatchToReceiver(GameCoordinatorMessage.CreateFromByteArray((GameCoordinatorMessageType)type, protobuf, msg.payload));
        }

        [MessageReceiver(MessageType.ClientEmailAddrInfo)]
        private Task ReceiveEmailAddressInfo(CMsgClientEmailAddrInfo email)
        {
            CurrentUser.Email = email.email_address;
            CurrentUser.EmailValidated = email.email_is_validated;
            return Task.CompletedTask;
        }

        [MessageReceiver(MessageType.ClientAccountInfo)]
        private Task ReceiveAccountInfo(CMsgClientAccountInfo info)
        {
            CurrentUser.PlayerName = info.persona_name;
            return Task.CompletedTask;
        }

        [MessageReceiver(MessageType.ClientWalletInfoUpdate)]
        private Task ReceiveWallet(CMsgClientWalletInfoUpdate wallet)
        {
            CurrentUser.Wallet.Currency = (CurrencyCode)wallet.currency;
            CurrentUser.Wallet.Cents = wallet.balance64;
            CurrentUser.Wallet.CentsPending = wallet.balance64_delayed;
            return Task.CompletedTask;
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
                    _servers[type].Add(serverValue);
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
                _servers[ServerType.ConnectionManager].Union(servers);
            else
                _servers[ServerType.ConnectionManager] = servers.ToImmutableHashSet();

            return Task.CompletedTask;
        }
        
        [MessageReceiver(MessageType.ClientLogOnResponse)]
        private async Task AutoLoginToFriends(CMsgClientLogonResponse response)
        {
            if (response.eresult == 1 && GetConfig<SteamNetworkConfig>().AutoLoginFriends && SteamId.FromCommunityId(response.client_supplied_steamid).IsIndividualAccount)
            {
                var jobResponse = await SendJobAsync<CMsgPersonaChangeResponse>(NetworkMessage.CreateProtobufMessage(MessageType.ClientChangeStatus, new CMsgClientChangeStatus { persona_state = (uint)PersonaState.Online }));

                if (jobResponse.result == 1)
                {
                    CurrentUser.Status = PersonaState.Online;
                }
                else
                {
                    await NetLog.WarningAsync($"Auto friends login did not complete successfully: {(Result)jobResponse.result}");
                }
            }
        }
    }
}
