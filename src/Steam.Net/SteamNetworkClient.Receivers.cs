using Steam.Net.Messages;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Messages.Structs;
using Steam.Net.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        [MessageReceiver(MessageType.Multi)]
        private async Task ReceiveMulti(Multiple multi)
        {
            byte[] payload = multi.MessageBody;
            if (multi.SizeUnzipped > 0)
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

                    var _ = DispatchData(subData).ContinueWith(ContinueDispatch);
                }
            }
        }

        [MessageReceiver(MessageType.ChannelEncryptRequest)]
        private async Task ReceiveEncryptRequest(ChannelEncryptRequest encryptRequest)
        {
            LogVerbose(_source, $"Encrypting channel on protocol version {encryptRequest.ProtocolVersion} in universe {encryptRequest.Universe}");
            SteamId = SteamId.CreateAnonymousUser(encryptRequest.Universe);

            byte[] challange = encryptRequest.Challenge.Length >= 16 ? encryptRequest.Challenge : null;
            byte[] publicKey = UniverseUtils.GetPublicKey(encryptRequest.Universe);
            if (publicKey == null)
            {
                LogCritical(_source, $"Cannot find public key for universe {encryptRequest.Universe}");
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

            // no need to use streams and append zeros on the end when they're already the default value
            _encryption = challange != null ? (IEncryptor)new HmacEncryptor(tempSessionKey) : new SimpleEncryptor(tempSessionKey);
            _encryptionPending = true;

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
                _encryptionPending = false;
                LogDebug(_source, "Channel encrypted");
                await ConnectedAsync();
            }
        }

        [MessageReceiver(MessageType.ClientLogOnResponse)]
        private Task ReceiveLogon(NetworkMessage messsage)
        {
            LogonResponse response = messsage.Deserialize<LogonResponse>();
            if (response.Result != 1)
                LogInfo(_source, $"Logon denied: {(Result)response.Result}. Expect to disconnect");

            switch ((Result)response.Result)
            {
                case Result.OK:
                    GetConfig<SteamNetworkConfig>().CellId = response.CellId;
                    SessionId = (messsage.Header as ClientHeader).SessionId;
                    SteamId = (messsage.Header as ClientHeader).SteamId;
                    _continueLogin = false;
                    LogInfo(_source, $"Logged in to Steam with session Id {SessionId} and steam ID {SteamId.ToSteam3Id()}");
                    LoggedOn?.Invoke(this, EventArgs.Empty);

                    _heartbeatCancel?.Cancel();
                    _heartbeatCancel = new CancellationTokenSource();
                    _heartBeatTask = RunHeartbeatAsync(response.OutOfGameHeartbeatSeconds * 1000, _heartbeatCancel.Token);
                    break;
                case Result.AccountLoginDeniedNeedTwoFactor:
                case Result.AccountLogonDenied:
                case Result.AccountLogonDeniedVerifiedEmailRequired:
                    _previousLogonResponse = response;
                    _continueLogin = true;
                    break;
                default:
                    LoginRejected?.Invoke(this, EventArgs.Empty);
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
