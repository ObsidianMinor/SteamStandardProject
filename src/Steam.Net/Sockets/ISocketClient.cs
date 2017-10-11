using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    public interface ISocketClient : IDisposable
    {
        IPAddress LocalIp { get; }
        
        void SetCancellationTtoken(CancellationToken cancellationToken);
        Task ConnectAsync(IPEndPoint endpoint, int timeout);
        Task DisconnectAsync();
        Task SendAsync(byte[] data);
    }
}
