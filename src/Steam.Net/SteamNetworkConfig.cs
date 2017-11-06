using Steam.Net.Sockets;
using Steam.Web;
using System;
using System.Collections.Generic;
using System.Net;

namespace Steam.Net
{
    public class SteamNetworkConfig : SteamWebConfig
    {
        private static IReadOnlyDictionary<Universe, IReadOnlyCollection<IPEndPoint>> GetEndpoints()
        {
            return new Dictionary<Universe, IReadOnlyCollection<IPEndPoint>>()
            {
                {
                    Universe.Public,
                    new List<IPEndPoint>()
                    {
                        new IPEndPoint(IPAddress.Parse("162.254.193.6"), 27017),
                        new IPEndPoint(IPAddress.Parse("162.254.193.6"), 27018),
                        new IPEndPoint(IPAddress.Parse("162.254.193.6"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.193.6"), 27020),
                        new IPEndPoint(IPAddress.Parse("162.254.193.6"), 27021),
                        new IPEndPoint(IPAddress.Parse("162.254.193.7"), 27017),
                        new IPEndPoint(IPAddress.Parse("162.254.193.7"), 27018),
                        new IPEndPoint(IPAddress.Parse("162.254.193.7"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.193.7"), 27020),
                        new IPEndPoint(IPAddress.Parse("162.254.193.7"), 27021),
                        new IPEndPoint(IPAddress.Parse("162.254.193.46"), 27017),
                        new IPEndPoint(IPAddress.Parse("162.254.193.46"), 27018),
                        new IPEndPoint(IPAddress.Parse("162.254.193.46"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.193.46"), 27020),
                        new IPEndPoint(IPAddress.Parse("162.254.193.46"), 27021),
                        new IPEndPoint(IPAddress.Parse("162.254.193.47"), 27017),
                        new IPEndPoint(IPAddress.Parse("162.254.193.47"), 27018),
                        new IPEndPoint(IPAddress.Parse("162.254.193.47"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.193.47"), 27020),
                        new IPEndPoint(IPAddress.Parse("162.254.193.47"), 27021),
                        new IPEndPoint(IPAddress.Parse("208.78.164.10"), 27017),
                        new IPEndPoint(IPAddress.Parse("208.78.164.10"), 27018),
                        new IPEndPoint(IPAddress.Parse("208.78.164.10"), 27019),
                        new IPEndPoint(IPAddress.Parse("208.78.164.12"), 27017),
                        new IPEndPoint(IPAddress.Parse("208.78.164.12"), 27018),
                        new IPEndPoint(IPAddress.Parse("208.78.164.12"), 27019),
                        new IPEndPoint(IPAddress.Parse("208.78.164.13"), 27017),
                        new IPEndPoint(IPAddress.Parse("208.78.164.13"), 27018),
                        new IPEndPoint(IPAddress.Parse("208.78.164.13"), 27019),
                        new IPEndPoint(IPAddress.Parse("208.78.164.14"), 27017),
                        new IPEndPoint(IPAddress.Parse("208.78.164.14"), 27018),
                        new IPEndPoint(IPAddress.Parse("208.78.164.14"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.195.44"), 27017),
                        new IPEndPoint(IPAddress.Parse("162.254.195.44"), 27018),
                        new IPEndPoint(IPAddress.Parse("162.254.195.44"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.195.44"), 27020),
                        new IPEndPoint(IPAddress.Parse("162.254.195.44"), 27021),
                        new IPEndPoint(IPAddress.Parse("162.254.195.45"), 27017),
                        new IPEndPoint(IPAddress.Parse("162.254.195.45"), 27018),
                        new IPEndPoint(IPAddress.Parse("162.254.195.45"), 27019),
                        new IPEndPoint(IPAddress.Parse("162.254.195.45"), 27020),
                        new IPEndPoint(IPAddress.Parse("162.254.195.45"), 27021),
                    }
                },
                {
                    Universe.Beta,
                    new List<IPEndPoint>()
                    {
                        new IPEndPoint(IPAddress.Parse("172.16.3.106"), IPEndPoint.MinPort),
                        new IPEndPoint(IPAddress.Parse("172.16.3.36"), 27017),
                    }
                },
                {
                    Universe.Internal,
                    new List<IPEndPoint>()
                    {
                        new IPEndPoint(IPAddress.Parse("172.16.2.200"), IPEndPoint.MinPort),
                        new IPEndPoint(IPAddress.Parse("172.16.2.200"), IPEndPoint.MinPort),
                        new IPEndPoint(IPAddress.Parse("172.16.2.201"), IPEndPoint.MinPort),
                        new IPEndPoint(IPAddress.Parse("172.16.2.202"), IPEndPoint.MinPort),
                        new IPEndPoint(IPAddress.Parse("172.16.2.203"), IPEndPoint.MinPort),
                        new IPEndPoint(IPAddress.Parse("172.16.2.207"), IPEndPoint.MinPort),
                    }
                },
                {
                    Universe.Dev,
                    new List<IPEndPoint>()
                    {
                        new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27017)
                    }
                }
            };
        }

        private static IReadOnlyDictionary<Universe, IReadOnlyCollection<Uri>> GetWebSockets()
        {
            return new Dictionary<Universe, IReadOnlyCollection<Uri>>()
            {
                {
                    Universe.Public,
                    new List<Uri>()
                    {
                        new Uri("wss://cm01-ord.cm.steampowered.com:443"),
                        new Uri("wss://cm02-ord.cm.steampowered.com:443"),
                        new Uri("wss://cm03-ord.cm.steampowered.com:443"),
                        new Uri("wss://cm04-ord.cm.steampowered.com:443"),
                        new Uri("wss://CM02-IAD.cm.steampowered.com:443"),
                        new Uri("wss://CM04-IAD.cm.steampowered.com:443"),
                        new Uri("wss://CM05-IAD.cm.steampowered.com:443"),
                        new Uri("wss://CM06-IAD.cm.steampowered.com:443"),
                        new Uri("wss://cm01-lax.cm.steampowered.com:443"),
                        new Uri("wss://cm02-lax.cm.steampowered.com:443")
                    }
                },
                {
                    Universe.Beta,
                    new List<Uri>()
                    {
                        new Uri("wss://steam3beta-mds.valvesoftware.com:443"),
                        new Uri("wss://steam3-beta8.valvesoftware.com:443")
                    }
                },
                {
                    Universe.Internal,
                    new List<Uri>()
                    {
                        new Uri("wss://172.16.2.199:443"),
                        new Uri("wss://172.16.2.200:443"),
                        new Uri("wss://172.16.2.201:443"),
                        new Uri("wss://172.16.2.202:443"),
                        new Uri("wss://172.16.2.203:443"),
                        new Uri("wss://172.16.2.207:443")
                    }
                },
                {
                    Universe.Dev,
                    new List<Uri>()
                    {
                        new Uri("wss://127.0.0.1:3443")
                    }
                }
            };
        }

        /// <summary>
        /// Sets the socket client provider for this client. The default is a TCP client provider
        /// </summary>
        public Func<ISocketClient> SocketClient { get; set; } = () => new DefaultTcpSocketClient();

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

        /// <summary>
        /// Sets the timeout period in milliseconds for receiver methods to complete exection asynchronously
        /// </summary>
        public int ReceiveMethodTimeout { get; set; } = -1;

        /// <summary>
        /// Represents the provider to get the default web socket implementation
        /// </summary>
        /// <returns></returns>
        public static ISocketClient DefaultWebSocketProvider() => new DefaultWebSocketClient();

        /// <summary>
        /// Gets a dictionary with a collection of default IP endpoints used in the various Steam universes
        /// </summary>
        /// <remarks>
        /// Yes I got them from steamclient.dylib
        /// </remarks>
        public static IReadOnlyDictionary<Universe, IReadOnlyCollection<IPEndPoint>> DefaultEndpoints => GetEndpoints();
        /// <summary>
        /// Gets a dictionary with a collection of default web socket endpoints used in the various Steam universes
        /// </summary>
        public static IReadOnlyDictionary<Universe, IReadOnlyCollection<Uri>> DefaultWebSocketEndpoints => GetWebSockets();
    }
}
