using System;
using System.IO;
using System.Threading.Tasks;

namespace Steam.Net.Sockets
{
    public delegate ISocketClient SocketClientProvider(Func<byte[], Task> dataReceivedFunc, Func<Task> connectedFunc, Func<Exception, Task> disconnectedFunc);
}
