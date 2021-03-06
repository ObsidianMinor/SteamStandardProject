﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    internal class DefaultWebSocketClient : IWebSocketClient, IDisposable
    {
        public const int ReceiveChunkSize = 16 * 1024; //16KB
        public const int SendChunkSize = 4 * 1024; //4KB
        private const int HR_TIMEOUT = -2147012894;

        public event AsyncEventHandler<DataReceivedEventArgs> MessageReceived;
        public event AsyncEventHandler<SocketDisconnectedEventArgs> Disconnected;

        private readonly SemaphoreSlim _lock;
        private readonly Dictionary<string, string> _headers;
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
            _headers = new Dictionary<string, string>();
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

        IPAddress ISocketClient.LocalIp => IPAddress.Any;

        Task ISocketClient.ConnectAsync(IPEndPoint endpoint) => throw new NotSupportedException();

        public async Task ConnectAsync(Uri uri)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ConnectInternalAsync(uri).ConfigureAwait(false);
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
            foreach (var header in _headers)
            {
                if (header.Value != null)
                    _client.Options.SetRequestHeader(header.Key, header.Value);
            }

            await _client.ConnectAsync(host, _cancelToken).ConfigureAwait(false);
            _task = RunAsync(_cancelToken);
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
                    try { await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", new CancellationToken()).ConfigureAwait(false); }
                    catch { }
                }
                try { _client.Dispose(); }
                catch { }

                _client = null;
            }
        }
        private async Task OnClosed(Exception ex)
        {
            if (_isDisconnecting)
                return; //Ignore, this disconnect was requested.

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectInternalAsync(false).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
            await Disconnected.InvokeAsync(this, new SocketDisconnectedEventArgs(ex)).ConfigureAwait(false);
        }

        public void SetHeader(string key, string value)
        {
            _headers[key] = value;
        }
        public void SetCancellationToken(CancellationToken cancelToken)
        {
            _parentToken = cancelToken;
            _cancelToken = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _cancelTokenSource.Token).Token;
        }

        public async Task SendAsync(byte[] data)
        {
            int count = data.Length;
            int index = 0;
            await _lock.WaitAsync().ConfigureAwait(false);
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
                    
                    await _client.SendAsync(new ArraySegment<byte>(data, index, count), WebSocketMessageType.Binary, isLast, _cancelToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _lock.Release();
            }
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
                        throw new WebSocketClosedException((int)socketResult.CloseStatus, socketResult.CloseStatusDescription);

                    if (!socketResult.EndOfMessage)
                    {
                        //This is a large message (likely just READY), lets create a temporary expandable stream
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
#if MSTRYBUFFER
                            if (stream.TryGetBuffer(out var streamBuffer))
                                result = streamBuffer.Array;
                            else
                                result = stream.ToArray();
#else
                            result = stream.GetBuffer();
#endif
                        }
                    }
                    else
                    {
                        //Small message
                        result = new byte[socketResult.Count];
                        Buffer.BlockCopy(buffer.Array, 0, result, 0, socketResult.Count);
                    }

                    if (socketResult.MessageType == WebSocketMessageType.Text)
                    {
                        throw new NotSupportedException();
                    }
                    else
                        await MessageReceived.InvokeAsync(this, new DataReceivedEventArgs(result)).ConfigureAwait(false);
                }
            }
            catch (Win32Exception ex) when (ex.HResult == HR_TIMEOUT)
            {
                var _ = OnClosed(new Exception("Connection timed out.", ex));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                //This cannot be awaited otherwise we'll deadlock when DiscordApiClient waits for this task to complete.
                var _ = OnClosed(ex);
            }
        }
    }
}
