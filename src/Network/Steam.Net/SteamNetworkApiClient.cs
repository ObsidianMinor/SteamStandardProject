using Steam.Common;
using Steam.Common.Logging;
using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Messages.Serialization;
using Steam.Net.Messages.Structs;
using Steam.Net.Sockets;
using Steam.Net.Utilities;
using Steam.Web.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    /// <summary>
    /// Provides a centalized model for interacting with the Steam network
    /// </summary>
    /// <remarks>
    /// Steam.Net is built like a MVVM application, views are objects such as User, License, Clan, and even the SteamNetworkClient. 
    /// The view model is the SteamNetworkApiClient.
    /// Finally the model is the provided ISocketClient. The model has one job: send and receive data. It does not interact with the data in any way, it does not need to encrypt the data or check for errors in the data.
    /// </remarks>
    internal partial class SteamNetworkApiClient : SteamWebApiClient
    {
        private const uint _currentProtocolVer = 65579;
        private const uint _obfuscationMask = 0xBAADF00D;

        private readonly SteamNetworkConfig _config;
        private readonly SemaphoreSlim _stateLock;
        private readonly JobManager JobManager;
        private readonly ConnectionManager _connection;
        private readonly ClientState _state;
        private readonly ISocketClient _socket;
        private readonly bool _isWebSocket;
        private readonly LogManager _apiLog;

        private int _connectTimeout;
        private uint _defaultCellId;
        private bool _encryptionPending = false;
        private IEncryptor _encryption;
        private IPAddress _localIp;

        private ServerList _currentServers;
        private bool _firstConnect = true;
        private Task _heartBeatTask;
        private CancellationTokenSource _heartbeatCancel;
        private bool _continueLogin;

        // login continuation
        private ClientProtobufMessage<LogonRequest> _logonContinuation;
        private LogonResponse _previousLogonResponse;

        internal ClientState State => _state;

        internal ConnectionState ConnectionState { get; private set; }

        internal SteamNetworkApiClient(SteamNetworkConfig config, LogManager log) : base(config)
        {
            _socket = config.SocketClient;
            _socket.ReceivedData += ReceiveAsync;
            _socket.Disconnected += OnDisconnected;
            _connectTimeout = config.NetworkConnectionTimeout;
            _isWebSocket = _socket is IWebSocketClient;
            _defaultCellId = config.CellId;
            _stateLock = new SemaphoreSlim(1, 1);
            _state = new ClientState();
            _config = config;
            _apiLog = log;
            _currentServers = new ServerList(_defaultCellId, this, _config.WebSockets ?? Enumerable.Empty<Uri>(), _config.ConnectionManagers ?? Enumerable.Empty<IPEndPoint>());
            JobManager = new JobManager(_apiLog.CreateLinkedManager("Jobs"));
            _connection = new ConnectionManager(_stateLock, _apiLog.CreateLinkedManager("CM"), _config.NetworkConnectionTimeout, OnConnectingAsync, OnDisconnectingAsync, x => DisconnectedEvent += x);
            _connection.Connected += OnConnected;
            _apiLog.Log += (src, message) =>
            {
                LogEvent?.Invoke(this, message);
            };
        }

        /// <summary>
        /// Connects this client to a connection manager 
        /// </summary>
        /// <returns></returns>
        private async Task OnConnectingAsync()
        {
            if (_continueLogin && LoginActionRequestedEvent.GetInvocationList().Length != 0)
            {
                await _apiLog.LogDebugAsync("Previous login failed and LoginActionRequested has subscribers, running event before resuming connection").ConfigureAwait(false);
                LoginActionRequestedEvent.Invoke(this, _previousLogonResponse);
            }

            if (_firstConnect)
            {
                await _apiLog.LogInfoAsync($"Steam.Net v0.1.0").ConfigureAwait(false);
                _firstConnect = false;
            }

            if (_isWebSocket)
            {
                IWebSocketClient webSocket = _socket as IWebSocketClient;
                do
                {
                    Uri endpoint = await _currentServers.GetCurrentWebSocketConnectionManagerAsync();
                    try
                    {
                        await webSocket.ConnectAsync(endpoint);
                        return;
                    }
                    catch (WebSocketException e)
                    {
                        await _apiLog.LogErrorAsync($"Could not connect to {endpoint}: {e.GetType()} {e.Message}").ConfigureAwait(false);
                        _currentServers.MarkCurrentWebSocket();
                    }
                } while (_currentServers.HasValidWebSocketManagers);
            }
            else
            {
                do
                {
                    IPEndPoint endpoint = await _currentServers.GetCurrentConnectionManagerAsync();
                    try
                    {
                        await _socket.ConnectAsync(endpoint, _connectTimeout);
                        return;
                    }
                    catch (SocketException e)
                    {
                        await _apiLog.LogErrorAsync($"Could not connect to {endpoint}: {e.GetType()} {e.Message}").ConfigureAwait(false);
                        _currentServers.MarkCurrent();
                    }
                } while (_currentServers.HasValidManagers);
            }

            throw new InvalidOperationException("Could not connect to a remote endpoint. All provided, retrieved, and fallback endpoints failed to connect");
        }

        private async Task OnDisconnectingAsync(Exception ex)
        {
            await _socket.DisconnectAsync().ConfigureAwait(false);
            _localIp = null;

            ConnectionState = ConnectionState.Disconnected;
        }

        private async void OnConnected(object sender, EventArgs e)
        {
            _localIp = _socket.LocalIp;

            if (_isWebSocket)
                await Connected();
        }

        private void OnDisconnected(object sender, Exception exception)
        {
            _encryption = null;
            DisconnectedEvent?.Invoke(this, exception);
            _connection.Error(exception);
        }

        internal async Task LoginAsync(string username, string password, uint? loginId, string authCode, string twoFactorCode, string loginKey, bool? shouldRememberPassword, bool requestSteam2Ticket, byte[] sentryFileHash, OsType osType, string language, uint accountId, AccountType accountType)
        {
            uint instance = 0;
            if (osType == OsType.PS3 && accountId != 0)
                instance = 2;
            else if (accountType != AccountType.AnonUser)
                instance = 1;
            
            await _apiLog.LogInfoAsync($"Logging in as {username ?? (instance == 0 ? "an anonymous user" : "a console user")}");
            Task<byte[]> machineIdTask = HardwareUtils.GetMachineId(); // while we set up the logon object, we will start to get the machine ID
            
            var logon = new ClientProtobufMessage<LogonRequest>(MessageType.ClientLogon)
            {
                SteamId = new SteamId(accountId, _state.SteamId.AccountUniverse, accountType, instance)
            };

            if (accountType != AccountType.AnonUser)
            {
                logon.Body.account_name = username;
                logon.Body.password = password;
                logon.Body.should_remember_password = shouldRememberPassword ?? false;
                logon.Body.steam2_ticket_request = requestSteam2Ticket;
                logon.Body.auth_code = authCode;
                logon.Body.two_factor_code = twoFactorCode;
                logon.Body.login_key = loginKey;
                logon.Body.sha_sentryfile = sentryFileHash;
                logon.Body.eresult_sentryfile = sentryFileHash is null ? (int)Result.FileNotFound : (int)Result.OK;
                logon.Body.client_package_version = 1771;
                logon.Body.obfustucated_private_ip = loginId ?? HardwareUtils.GetIpAddress(_socket.LocalIp) ^ _obfuscationMask;
                logon.Body.supports_rate_limit_response = true;
            }

            logon.Body.protocol_version = 65579;
            logon.Body.client_os_type = (uint)osType;
            logon.Body.client_language = language;
            logon.Body.cell_id = _state.CellId;
            logon.Body.machine_id = await machineIdTask;

            await SendAsync(logon).ConfigureAwait(false);

            _logonContinuation = logon;
        }

        public Task SetLoginContinuationAsync(string code)
        {
            if (_logonContinuation == null || _previousLogonResponse == null)
                throw new InvalidOperationException("Can't continue previous logon, original logon request or response doesn't exist");

            Result logonResult = (Result)_previousLogonResponse.Result;
            if (logonResult == Result.AccountLogonDeniedVerifiedEmailRequired)
            {
                _logonContinuation.Body.auth_code = code;
            }
            else if (logonResult == Result.AccountLoginDeniedNeedTwoFactor || logonResult == Result.AccountLogonDenied)
            {
                _logonContinuation.Body.two_factor_code = code;
            }
            else
                throw new InvalidOperationException("Previous logon error was not denied for verification email or two factor failure");

            _previousLogonResponse = null;
            return Task.CompletedTask;
        }

        internal async Task ContinueLoginAsync()
        {
            await SendAsync(_logonContinuation);
        }
        
        internal async Task StartAsync()
        {
            await _connection.StartAsync().ConfigureAwait(false);
        }
        
        internal async Task StopAsync()
        {
            await _connection.StartAsync().ConfigureAwait(false);
        }

        internal async Task<PicsChanges> GetPicsChangesAsync(uint changeNumber, bool sendAppChangeList, bool sendPackageChangeList)
        {
            var request = new ClientProtobufMessage<PicsChangesSinceRequest>(MessageType.ClientPICSChangesSinceRequest);
            (Task<PicsChangesSinceResponse> task, SteamGuid jobId) = JobManager.AddJob<PicsChangesSinceResponse>();
            request.SourceJobId = jobId;

            request.Body.SinceChangeNumber = changeNumber;
            request.Body.SendAppInfoChanges = sendAppChangeList;
            request.Body.SendPackageInfoChanges = sendPackageChangeList;

            await SendAsync(request).ConfigureAwait(false);

            PicsChangesSinceResponse response = await task.ConfigureAwait(false);
            return PicsChanges.Create(response);
        }

        internal async Task SendHearbeatAsync()
        {
            await SendAsync(new ClientProtobufMessage<Heartbeat>(MessageType.ClientHeartBeat));
        }

        internal async Task SendChatMessageAsync(SteamId chat, string message, ChatEntryType type)
        {
            if(chat.AccountType == AccountType.Clan)
                chat = new SteamId(chat.AccountId, chat.AccountUniverse, AccountType.Chat, SteamId.ClanChatInstance);

            var chatMessage = new ClientStructMessage<ClientChatMessage>();
            chatMessage.Body.Author = _state.CurrentUser.Id;
            chatMessage.Body.Room = chat;
            chatMessage.Body.Type = type;

            chatMessage.Payload = Encoding.UTF8.GetBytes(message + '\0');

            await SendAsync(chatMessage).ConfigureAwait(false);
        }
        
        internal async Task EnterChatRoomAsync(SteamId chat)
        {
            var request = new ClientStructMessage<JoinChat>(MessageType.ClientJoinChat);
            if (chat.AccountType == AccountType.Clan)
                chat = new SteamId(chat.AccountId, chat.AccountUniverse, AccountType.Chat, SteamId.ClanChatInstance);

            request.Body.ChatId = chat;
            await SendAsync(request).ConfigureAwait(false);
        }

        internal async Task LeaveChatRoomAsync(SteamId chat)
        {
            if (chat.AccountType == AccountType.Clan)
                chat = new SteamId(chat.AccountId, chat.AccountUniverse, AccountType.Chat, SteamId.ClanChatInstance);

            var leaveChat = new ClientStructMessage<ChatMemberInfo>(MessageType.ClientChatMemberInfo);

            leaveChat.Body.ChatId = chat;
            leaveChat.Body.Type = ChatInfoType.StateChange;

            byte[] steamId = BitConverter.GetBytes(_state.CurrentUser.Id);
            byte[] state = BitConverter.GetBytes((uint)ChatMemberStateChange.Left);
            CopyArraysToPayload(leaveChat, steamId, state, steamId);

            await SendAsync(leaveChat).ConfigureAwait(false);
        }

        private static void CopyArraysToPayload(IPayload payload, params byte[][] arrays)
        {
            payload.Payload = arrays.SelectMany(a => a).ToArray();
        }
        
        private async Task RunHeartbeatAsync(int interval, CancellationToken token)
        {
            try
            {
                await _apiLog.LogDebugAsync($"Heartbeat started on a {interval} ms interval").ConfigureAwait(false);
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token).ConfigureAwait(false);

                    await SendHearbeatAsync().ConfigureAwait(false);
                    await _apiLog.LogDebugAsync("Sent heartbeat");
                }
            }
            catch (OperationCanceledException)
            {
                await _apiLog.LogDebugAsync("Heartbeat stopped").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _apiLog.LogErrorAsync($"The heartbeat task encountered an unknown exception: {ex}").ConfigureAwait(false);
            }
        }

        internal byte[] Decrypt(byte[] data, bool isMulti)
        {
            if (_encryption != null && !_encryptionPending && !isMulti)
                return _encryption.Decrypt(data);
            else
                return data;
        }

        private async Task SendAsync(IMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (message is IClientMessage clientMessage)
            {
                if (_state.SessionId > 0)
                    clientMessage.SessionId = _state.SessionId;

                if (_state.SteamId > 0 && clientMessage.SteamId == 0) // check if we haven't already set the ID like in login method
                    clientMessage.SteamId = _state.SteamId;
            }

            byte[] data = await MessageSerializer.SerializeMessageAsync(message);

            if (_encryption != null && !_encryptionPending)
                data = _encryption.Encrypt(data);

            await _socket.SendAsync(data).ConfigureAwait(false);
        }

        private void ReceiveAsync(object sender, byte[] data) => ReceiveAsync(data, false);

        private async void ReceiveAsync(byte[] data, bool isMulti)
        {
            data = Decrypt(data, isMulti);

            (var type, var isProtobuf) = BitConverter.ToUInt32(data, 0).SplitMessage();

            switch (type)
            {
                case MessageType.ClientLogOnResponse:
                    {
                        ClientProtobufMessage<LogonResponse> response = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<LogonResponse>, LogonResponse>(data);
                        if (response.Body.Result != 1)
                            await _apiLog.LogInfoAsync($"Logon denied: {(Result)response.Body.Result}. Expect to disconnect");

                        switch ((Result)response.Body.Result)
                        {
                            case Result.OK:
                                _state.SetSessionInfo(response.Body.CellId, response.SessionId, response.SteamId, (AccountFlags)response.Body.AccountFlags);
                                _continueLogin = false;
                                await _apiLog.LogInfoAsync($"Logged in to Steam with session Id {_state.SessionId} and steam ID {_state.SteamId.ToSteam3Id()}").ConfigureAwait(false);
                                LoggedInEvent?.Invoke(this, new EventArgs());

                                _heartbeatCancel?.Cancel();
                                _heartbeatCancel = new CancellationTokenSource();
                                _heartBeatTask = RunHeartbeatAsync(response.Body.OutOfGameHeartbeatSeconds * 1000, _heartbeatCancel.Token);
                                break;
                            case Result.AccountLoginDeniedNeedTwoFactor:
                            case Result.AccountLogonDenied:
                            case Result.AccountLogonDeniedVerifiedEmailRequired:
                                _previousLogonResponse = response.Body;
                                _continueLogin = true;
                                break;
                            default:
                                LoginRejectedEvent?.Invoke(this, response.Body);
                                break;
                        }
                    }
                    break;
                case MessageType.ClientLoggedOff:
                    {
                        await _apiLog.LogInfoAsync("Logged off of Steam").ConfigureAwait(false);
                        _state.RemoveSessionInfo();

                        _heartbeatCancel?.Cancel();
                        _heartBeatTask = null; // set the field null so garbage collection can get the cancelled task

                        if (isProtobuf)
                        {
                            ClientProtobufMessage<LoggedOff> response = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<LoggedOff>, LoggedOff>(data);
                            Result result = (Result)response.Body.Result;
                            LoggedOffEvent?.Invoke(this, result);
                        }
                    }
                    break;
                case MessageType.ClientCMList:
                    {
                        var response = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<ConnectionManagerList>, ConnectionManagerList>(data);
                        _state.SetWebSockets(response.Body.WebSocketAddresses.Select(s =>
                        {
                            var split = s.Split(new[] { ':' }, 2);
                            return new UriBuilder("wss", split[0], int.Parse(split[1])).Uri;
                        }));

                        List<IPEndPoint> endpoints = new List<IPEndPoint>();
                        for(int i = 0; i < response.Body.ConnectionManagerAddresses.Count; i++)
                            endpoints.Add(new IPEndPoint(response.Body.ConnectionManagerAddresses[i], (int)response.Body.ConnectionManagerPorts[i]));

                        _state.SetServers(ServerType.ConnectionManager, endpoints);
                    }
                    break;
                case MessageType.JobHeartbeat:
                    {
                        var response = await MessageSerializer.DeserializeMessageAsync<ClientProtobufMessageHeader>(data).ConfigureAwait(false);
                        await JobManager.HeartbeatJob(response.TargetJobId).ConfigureAwait(false);
                    }
                    break;
                case MessageType.DestJobFailed:
                    {
                        var response = await MessageSerializer.DeserializeMessageAsync<ClientProtobufMessageHeader>(data).ConfigureAwait(false);
                        await JobManager.SetJobFail(response.TargetJobId, new DestinationJobFailedException(response.TargetJobId)).ConfigureAwait(false);
                    }
                    break;
                case MessageType.ClientPICSProductInfoResponse:
                    {
                        var response = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<PicsProductInfoResponse>, PicsProductInfoResponse>(data);
                        await JobManager.SetJobResult(response.TargetJobId, response.Body).ConfigureAwait(false);
                    }
                    break;
                case MessageType.ClientPICSChangesSinceResponse:
                    {
                        var response = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<PicsChangesSinceResponse>, PicsChangesSinceResponse>(data);
                        await JobManager.SetJobResult(response.TargetJobId, response.Body).ConfigureAwait(false);
                    }
                    break;
                case MessageType.ClientSessionToken:
                    {
                        var response = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<SessionToken>, SessionToken>(data);
                        _state.SessionToken = response.Body.Token;
                    }
                    break;
                case MessageType.ChannelEncryptRequest:
                    {
                        var encryptRequest = await MessageSerializer.DeserializeStructMessageAsync<StructMessage<ChannelEncryptRequest>, ChannelEncryptRequest>(data);
                        await _apiLog.LogVerboseAsync($"Encrypting channel on protocol version {encryptRequest.Body.ProtocolVersion} in universe {encryptRequest.Body.Universe}");
                        _state.SteamId = new SteamId(0, encryptRequest.Body.Universe);

                        byte[] challange = encryptRequest.Payload.Length >= 16 ? encryptRequest.Payload : null;
                        byte[] publicKey = UniverseUtils.GetPublicKey(encryptRequest.Body.Universe);
                        if (publicKey == null)
                        {
                            await _apiLog.LogCriticalAsync($"Cannot find public key for universe {encryptRequest.Body.Universe}");
                            throw new InvalidOperationException($"Public key does not exist for universe {encryptRequest.Body.Universe}");
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

                        byte[] keyHash = CryptoUtils.CrcHash(encryptedHandshake);

                        byte[] payload = new byte[encryptedHandshake.Length + keyHash.Length + 4];
                        Array.Copy(encryptedHandshake, payload, encryptedHandshake.Length);
                        Array.Copy(keyHash, 0, payload, encryptedHandshake.Length, keyHash.Length);
                        // no need to use streams and append zeros on the end when they're already the default value
                        _encryption = challange != null ? (IEncryptor)new HmacEncryptor(tempSessionKey) : new SimpleEncryptor(tempSessionKey);
                        _encryptionPending = true;

                        var encryptResponse = new StructMessage<ChannelEncryptResponse>(MessageType.ChannelEncryptResponse)
                        {
                            Payload = payload
                        };
                        await SendAsync(encryptResponse).ConfigureAwait(false);
                    }
                    break;
                case MessageType.ChannelEncryptResult:
                    {
                        var result = await MessageSerializer.DeserializeStructMessageAsync<StructMessage<ChannelEncryptResult>, ChannelEncryptResult>(data).ConfigureAwait(false);
                        if (result.Body.Result == Result.OK)
                        {
                            _encryptionPending = false;
                            await _apiLog.LogDebugAsync("Channel encrypted").ConfigureAwait(false);
                            await Connected();
                        }
                    }
                    break;
                case MessageType.Multi:
                    {
                        var multi = await MessageSerializer.DeserializeProtobufMessageAsync<ClientProtobufMessage<Multiple>, Multiple>(data).ConfigureAwait(false);

                        byte[] payload = multi.Body.MessageBody;
                        if (multi.Body.SizeUnzipped > 0)
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

                                ReceiveAsync(subData, true);
                            }
                        }
                    }
                    break;
                default:
                    await _apiLog.LogVerboseAsync($"Received unknown {(isProtobuf ? "protobuf message" : "message")} of type {type} ({(int)type}). Length is {data.Length} bytes");
                    break;
            }
        }

        private async Task Connected()
        {
            await _connection.CompleteAsync().ConfigureAwait(false);
            ConnectedEvent?.Invoke(this, new EventArgs());

            if (!_continueLogin)
            {
                CanLoginEvent?.Invoke(this, new EventArgs());
            }
            else
            {
                await _apiLog.LogDebugAsync("Can continue login from previous disconnect, sending previous info").ConfigureAwait(false);
                await ContinueLoginAsync().ConfigureAwait(false);
            }
        }
    }
}
