using System;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    public interface IWebSocketClient : ISocketClient
    {
        Task ConnectAsync(Uri host);
    }
}
