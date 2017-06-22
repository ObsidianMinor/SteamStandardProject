using Steam.Common.Logging;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net
{
    internal class ConnectionManager // borrowed from RogueException/Discord.Net, thanks for being MIT
    {
        public event EventHandler Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        private readonly SemaphoreSlim _stateLock;
        private readonly LogManager _logger;
        private readonly int _connectionTimeout;
        private readonly Func<Task> _onConnecting;
        private readonly Func<Exception, Task> _onDisconnecting;

        private TaskCompletionSource<bool> _connectionPromise, _readyPromise;
        private CancellationTokenSource _combinedCancelToken, _reconnectCancelToken, _connectionCancelToken;
        private Task _task;

        public ConnectionState State { get; private set; }
        public CancellationToken CancelToken { get; private set; }

        internal ConnectionManager(SemaphoreSlim stateLock, LogManager logger, int connectionTimeout,
            Func<Task> onConnecting, Func<Exception, Task> onDisconnecting, Action<EventHandler<Exception>> clientDisconnectHandler)
        {
            _stateLock = stateLock;
            _logger = logger;
            _connectionTimeout = connectionTimeout;
            _onConnecting = onConnecting;
            _onDisconnecting = onDisconnecting;

            clientDisconnectHandler((src, ex) =>
            {
                if (ex != null)
                {
                    Error(new Exception("Socket connection was closed", ex));
                }
                else
                    Error(new Exception("Socket connection was closed"));
            });
        }

        public virtual async Task StartAsync()
        {
            await AcquireConnectionLock().ConfigureAwait(false);
            var reconnectCancelToken = new CancellationTokenSource();
            _reconnectCancelToken = reconnectCancelToken;
            _task = Task.Run(async () =>
            {
                try
                {
                    Random jitter = new Random();
                    int nextReconnectDelay = 1000;
                    while (!reconnectCancelToken.IsCancellationRequested)
                    {
                        try
                        {
                            await ConnectAsync(reconnectCancelToken).ConfigureAwait(false);
                            nextReconnectDelay = 1000; //Reset delay
                            await _connectionPromise.Task.ConfigureAwait(false);
                        }
                        catch (OperationCanceledException ex)
                        {
                            Cancel(); //In case this exception didn't come from another Error call
                            await DisconnectAsync(ex, !reconnectCancelToken.IsCancellationRequested).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Error(ex); //In case this exception didn't come from another Error call
                            if (!reconnectCancelToken.IsCancellationRequested)
                            {
                                await _logger.LogWarningAsync(ex.ToString()).ConfigureAwait(false);
                                await DisconnectAsync(ex, true).ConfigureAwait(false);
                            }
                            else
                            {
                                await _logger.LogErrorAsync(ex.ToString()).ConfigureAwait(false);
                                await DisconnectAsync(ex, false).ConfigureAwait(false);
                            }
                        }

                        if (!reconnectCancelToken.IsCancellationRequested)
                        {
                            //Wait before reconnecting
                            await Task.Delay(nextReconnectDelay, reconnectCancelToken.Token).ConfigureAwait(false);
                            nextReconnectDelay = (nextReconnectDelay * 2) + jitter.Next(-250, 250);
                            if (nextReconnectDelay > 60000)
                                nextReconnectDelay = 60000;
                        }
                    }
                }
                finally { _stateLock.Release(); }
            });
        }
        public virtual async Task StopAsync()
        {
            Cancel();
            var task = _task;
            if (task != null)
                await task.ConfigureAwait(false);
        }

        private async Task ConnectAsync(CancellationTokenSource reconnectCancelToken)
        {
            _connectionCancelToken = new CancellationTokenSource();
            _combinedCancelToken = CancellationTokenSource.CreateLinkedTokenSource(_connectionCancelToken.Token, reconnectCancelToken.Token);
            CancelToken = _combinedCancelToken.Token;

            _connectionPromise = new TaskCompletionSource<bool>();
            State = ConnectionState.Connecting;
            await _logger.LogInfoAsync("Connecting").ConfigureAwait(false);

            try
            {
                var readyPromise = new TaskCompletionSource<bool>();
                _readyPromise = readyPromise;

                //Abort connection on timeout
                var cancelToken = CancelToken;
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(_connectionTimeout, cancelToken).ConfigureAwait(false);
                        readyPromise.TrySetException(new TimeoutException());
                    }
                    catch (OperationCanceledException) { }
                });

                await _onConnecting().ConfigureAwait(false);

                await _logger.LogInfoAsync("Connected").ConfigureAwait(false);
                State = ConnectionState.Connected;
                await _logger.LogDebugAsync("Raising Event").ConfigureAwait(false);
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
        }
        private async Task DisconnectAsync(Exception ex, bool isReconnecting)
        {
            if (State == ConnectionState.Disconnected) return;
            State = ConnectionState.Disconnecting;
            await _logger.LogInfoAsync("Disconnecting").ConfigureAwait(false);

            await _onDisconnecting(ex).ConfigureAwait(false);

            await _logger.LogInfoAsync("Disconnected").ConfigureAwait(false);
            State = ConnectionState.Disconnected;
            Disconnected?.Invoke(this, new DisconnectedEventArgs(isReconnecting, ex));
        }

        public Task CompleteAsync()
        {
            return Task.Run(() => _readyPromise.TrySetResult(true));
        }
        public async Task WaitAsync()
        {
            await _readyPromise.Task.ConfigureAwait(false);
        }

        public void Cancel()
        {
            _readyPromise?.TrySetCanceled();
            _connectionPromise?.TrySetCanceled();
            _reconnectCancelToken?.Cancel();
            _connectionCancelToken?.Cancel();
        }
        public void Error(Exception ex)
        {
            ex = ex ?? new SocketException((int)SocketError.ConnectionReset);
            _readyPromise.TrySetException(ex);
            _connectionPromise.TrySetException(ex);
            _connectionCancelToken?.Cancel();
        }
        public void CriticalError(Exception ex)
        {
            _reconnectCancelToken?.Cancel();
            Error(ex);
        }
        public void Reconnect()
        {
            _readyPromise.TrySetCanceled();
            _connectionPromise.TrySetCanceled();
            _connectionCancelToken?.Cancel();
        }
        private async Task AcquireConnectionLock()
        {
            while (true)
            {
                await StopAsync().ConfigureAwait(false);
                if (await _stateLock.WaitAsync(0).ConfigureAwait(false))
                    break;
            }
        }
    }
}
