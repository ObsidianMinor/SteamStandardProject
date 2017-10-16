using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    public interface ISocketClient : IDisposable
    {
        event Func<byte[], Task> MessageReceived;
        event Func<Exception, Task> Disconnected;

        IPAddress LocalIp { get; }
        
        void SetCancellationToken(CancellationToken cancellationToken);
        Task ConnectAsync(IPEndPoint endpoint);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);
    }
}
