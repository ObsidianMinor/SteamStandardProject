using Steam.Net.Sockets;
using Steam.Web;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Net
{
    public class SteamNetworkConfig : SteamWebConfig
    {
        /// <summary>
        /// Sets the socket client provider for this client. The default is a TCP client provider
        /// </summary>
        public SocketClientProvider SocketClient { get; set; } = (func, connect, disconnect) => new DefaultTcpSocketClient(func, connect, disconnect);

        /// <summary>
        /// The time in milliseconds to wait for a connection to complete before aborting
        /// </summary>
        public int NetworkConnectionTimeout { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the default cell Id to get when connecting to Steam
        /// </summary>
        public long CellId { get; set; } = 0;

        /// <summary>
        /// Gets or sets a list of connection manager endpoints which can be used before resorting to the web API
        /// </summary>
        public IEnumerable<IPEndPoint> ConnectionManagers { get; set; }

        /// <summary>
        /// Gets or sets a list of WebSocket connection managers which can be used before resorting to the web API
        /// </summary>
        public IEnumerable<Uri> WebSockets { get; set; }

        /// <summary>
        /// Gets or sets the default universe the Steam client will be in before logging in
        /// </summary>
        public Universe DefaultUniverse { get; set; } = Universe.Public;

        /// <summary>
        /// Sets the login ID to be used when the client logs in
        /// </summary>
        public long LoginId { get; set; } = -1;

        /// <summary>
        /// Sets the Steam language for this client
        /// </summary>
        public Language Language { get; set; } = Language.English;

        /// <summary>
        /// Sets the receiver method resolver. Changing this is recommended for advanced users only
        /// </summary>
        public Func<IReceiveMethodResolver> ReceiveMethodResolver { get; set; } = () => new DefaultReceiveMethodResolver();

        public static ISocketClient DefaultWebSocketProvider(Func<byte[], Task> dataReceivedFunc, Func<Task> connectedFunc, Func<Exception, Task> disconnectedFunc)
        {
            return new DefaultWebSocketClient(dataReceivedFunc, connectedFunc, disconnectedFunc);
        }
    }
}
