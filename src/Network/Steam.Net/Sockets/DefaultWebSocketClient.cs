using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    public sealed class DefaultWebSocketClient : IWebSocketClient
    {
        public const int ReceiveChunkSize = 16 * 1024; //16KB
        public const int SendChunkSize = 4 * 1024; //4KB
        private const int HR_TIMEOUT = -2147012894;

        private readonly SemaphoreSlim _lock;
        private ClientWebSocket _client;
        private Task _task;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken, _parentToken;
        private bool _isDisposed, _isDisconnecting;

        public DefaultWebSocketClient()
        {
            _lock = new SemaphoreSlim(1, 1);
            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = CancellationToken.None;
            _parentToken = CancellationToken.None;
        }

        public event EventHandler<byte[]> ReceivedData;
        public event EventHandler<Exception> Disconnected;
        public event EventHandler Connected;

        public IPAddress LocalIp => IPAddress.Any;

        public async Task ConnectAsync(Uri host)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ConnectInternalAsync(host).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }
        private async Task ConnectInternalAsync(Uri host)
        {
            await DisconnectInternalAsync().ConfigureAwait(false);

            _cancelTokenSource = new CancellationTokenSource();
            _cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _cancelTokenSource.Token).Token;

            _client = new ClientWebSocket();
            _client.Options.Proxy = null;
            _client.Options.KeepAliveInterval = TimeSpan.Zero;

            await _client.ConnectAsync(host, _cancelToken).ConfigureAwait(false);
            _task = RunAsync(_cancelToken);

            Connected?.Invoke(this, EventArgs.Empty);
        }

        public async Task DisconnectAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectInternalAsync().ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }
        private async Task DisconnectInternalAsync(bool isDisposing = false)
        {
            try { _cancelTokenSource.Cancel(false); } catch { }

            _isDisconnecting = true;
            try
            {
                await (_task ?? Task.Delay(0)).ConfigureAwait(false);
                _task = null;
            }
            finally { _isDisconnecting = false; }

            if (_client != null)
            {
                if (!isDisposing)
                {
                    try { await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", new CancellationToken()); }
                    catch { }
                }
                try { _client.Dispose(); }
                catch { }

                _client = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    DisconnectInternalAsync(true).GetAwaiter().GetResult();
                _isDisposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        Task ISocketClient.ConnectAsync(IPEndPoint endpoint, int timeout) => throw new NotSupportedException();

        public async Task SendAsync(byte[] data)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            int count = data.Length;
            int index = 0;
            try
            {
                if (_client == null) return;

                int frameCount = (int)Math.Ceiling((double)count / SendChunkSize);

                for (int i = 0; i < frameCount; i++, index += SendChunkSize)
                {
                    bool isLast = i == (frameCount - 1);

                    int frameSize;
                    if (isLast)
                        frameSize = count - (i * SendChunkSize);
                    else
                        frameSize = SendChunkSize;

                    var type = WebSocketMessageType.Binary;
                    await _client.SendAsync(new ArraySegment<byte>(data, index, count), type, isLast, _cancelToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public void SetCancellationTtoken(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task RunAsync(CancellationToken cancelToken)
        {
            var buffer = new ArraySegment<byte>(new byte[ReceiveChunkSize]);

            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult socketResult = await _client.ReceiveAsync(buffer, cancelToken).ConfigureAwait(false);
                    byte[] result;
                    int resultCount;

                    if (socketResult.MessageType == WebSocketMessageType.Close)
                        throw new WebSocketException((int)socketResult.CloseStatus, socketResult.CloseStatusDescription);

                    if (!socketResult.EndOfMessage)
                    {
                        using (var stream = new MemoryStream())
                        {
                            stream.Write(buffer.Array, 0, socketResult.Count);
                            do
                            {
                                if (cancelToken.IsCancellationRequested) return;
                                socketResult = await _client.ReceiveAsync(buffer, cancelToken).ConfigureAwait(false);
                                stream.Write(buffer.Array, 0, socketResult.Count);
                            }
                            while (socketResult == null || !socketResult.EndOfMessage);

                            //Use the internal buffer if we can get it
                            resultCount = (int)stream.Length;
                            if (stream.TryGetBuffer(out var streamBuffer))
                                result = streamBuffer.Array;
                            else
                                result = stream.ToArray();
                        }
                    }
                    else
                    {
                        //Small message
                        result = new byte[socketResult.Count];
                        Array.Copy(buffer.Array, result, socketResult.Count);
                    }

                    if (socketResult.MessageType == WebSocketMessageType.Text)
                    {
                        throw new NotImplementedException();
                    }
                    else
                        ReceivedData?.Invoke(this, result);
                }
            }
            catch (Win32Exception ex) when (ex.HResult == HR_TIMEOUT)
            {
                Disconnected?.Invoke(this, new TimeoutException("The operation timed out"));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Disconnected?.Invoke(this, ex);
            }
        }
    }
}
