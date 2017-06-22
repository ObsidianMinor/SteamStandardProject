using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    public interface ISocketClient : IDisposable
    {
        event EventHandler<byte[]> ReceivedData;
        event EventHandler<Exception> Disconnected;
        event EventHandler Connected;
        IPAddress LocalIp { get; }
        
        void SetCancellationTtoken(CancellationToken cancellationToken);
        Task ConnectAsync(IPEndPoint endpoint, int timeout);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);
    }
}
