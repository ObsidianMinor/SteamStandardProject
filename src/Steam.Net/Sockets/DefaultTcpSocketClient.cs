using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    internal class DefaultTcpSocketClient : ISocketClient
    {
        private readonly AsyncEvent<Func<byte[], Task>> _receiveDataEvent = new AsyncEvent<Func<byte[], Task>>();
        private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

        private readonly SemaphoreSlim _lock;
        private Socket _socket;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken, _parentToken = default;
        private Task _runTask;
        private bool _isDisconnecting;
        const int _tcpMagic = 0x31305456;

        public event Func<byte[], Task> MessageReceived
        {
            add => _receiveDataEvent.Add(value);
            remove => _receiveDataEvent.Remove(value);
        }

        public event Func<Exception, Task> Disconnected
        {
            add => _disconnectedEvent.Add(value);
            remove => _disconnectedEvent.Remove(value);
        }

        public DefaultTcpSocketClient()
        {
            _lock = new SemaphoreSlim(1, 1);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public IPAddress LocalIp => (_socket.LocalEndPoint as IPEndPoint).Address;
        
        public async Task ConnectAsync(IPEndPoint endpoint)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await ConnectInternalAsync(endpoint).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task ConnectInternalAsync(IPEndPoint endpoint)
        {
            await DisconnectInternalAsync().ConfigureAwait(false);

            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _cancellationTokenSource.Token).Token;

            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await Task.Run(() => _socket.Connect(endpoint), _cancellationToken).ConfigureAwait(false);
            
            _runTask = ReceiveAsync(_cancellationToken);
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

        private async Task DisconnectInternalAsync(bool disposing = false)
        {
            try
            {
                _cancellationTokenSource.Cancel(false);
            }
            catch { }

            _isDisconnecting = true;
            try
            {
                if (!disposing)
                    await (_runTask ?? Task.CompletedTask).ConfigureAwait(false);
            }
            finally { _isDisconnecting = false; }

            if(_socket != null)
            {
                try
                {
                    _socket.Close();
                    _socket.Dispose();
                }
                catch { }
                _socket = null;
            }
        }

        public async Task SendAsync(byte[] data)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_socket.Send(BitConverter.GetBytes(data.Length)) == 0 ||
                    _socket.Send(BitConverter.GetBytes(_tcpMagic)) == 0 ||
                    _socket.Send(data) == 0)
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(_parentToken, _cancellationTokenSource.Token).Token;
        }
        
        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            await Task.Yield(); // force ourselves to complete async

            Task _sentData = Task.CompletedTask;
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!_socket.Poll(-1, SelectMode.SelectRead))
                    {
                        continue;
                    }

                    if (!_socket.Connected)
                        throw new SocketException((int)SocketError.ConnectionReset);

                    byte[] bytes = new byte[8];
                    if (_socket.Receive(bytes, 0, bytes.Length, SocketFlags.None) == 0)
                        throw new SocketException((int)SocketError.ConnectionReset);

                    uint length = BitConverter.ToUInt32(bytes, 0);
                    uint magic = BitConverter.ToUInt32(bytes, 4);

                    if (magic != _tcpMagic)
                    {
                        throw new InvalidDataException("The provided packet does not have the correct magic value");
                    }

                    bytes = new byte[length];
                    ReceiveData(bytes, bytes.Length);

                    await _sentData.ConfigureAwait(false);

                    _sentData = _receiveDataEvent.InvokeAsync(bytes);
                }
            }
            catch (Exception ex)
            {
                var _ = OnClosed(ex);
            }
        }

        private async Task OnClosed(Exception ex)
        {
            if (_isDisconnecting)
                return;

            await _lock.WaitAsync().ConfigureAwait(false);
            try
            {
                await DisconnectInternalAsync(false).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
            await _disconnectedEvent.InvokeAsync(ex).ConfigureAwait(false);
        }

        private void ReceiveData(byte[] buffer, int length) // sometimes Steam sends data in batches and ReadAsync doesn't return that amount, so we need to loop until we get the whole packet
        {
            for (int remainingData = length, receivedData = 0; remainingData > 0; remainingData -= receivedData)
                receivedData = _socket.Receive(buffer, receivedData, remainingData, SocketFlags.None);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisconnectInternalAsync(true).GetAwaiter().GetResult();
                    _lock.Dispose();
                    _cancellationTokenSource.Dispose();
                }
                
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
