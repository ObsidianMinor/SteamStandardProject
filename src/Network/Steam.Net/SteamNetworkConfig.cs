using Steam.Net.Sockets;
using Steam.Web;
using System;
using System.Collections.Generic;
using System.Net;

namespace Steam.Net
{
    public class SteamNetworkConfig : SteamWebConfig
    {
        /// <summary>
        /// Sets the socket client for this client. The default is a TCP client
        /// </summary>
        public ISocketClient SocketClient { get; set; } = new DefaultTcpSocketClient();

        /// <summary>
        /// The time in milliseconds to wait for a connection to complete before aborting
        /// </summary>
        public int NetworkConnectionTimeout { get; set; } = 30000;

        /// <summary>
        /// Gets or sets the default cell Id to get when connecting to Steam
        /// </summary>
        public uint CellId { get; set; } = 0;

        /// <summary>
        /// Gets or sets a list of connection manager endpoints which can be used before resorting to a the web API
        /// </summary>
        public IEnumerable<IPEndPoint> ConnectionManagers { get; set; }

        /// <summary>
        /// Gets or sets a list of WebSocket connection managers which can be used before resorting to the web API
        /// </summary>
        public IEnumerable<Uri> WebSockets { get; set; }
    }
}
