using System;
using System.Net;

namespace Steam.Net
{
    /// <summary>
    /// Represents a Steam server that is either a URI or an IP endpoint
    /// </summary>
    public class Server : IEquatable<Server>
    {
        private readonly string _hostName;
        private readonly IPAddress _address;
        private readonly int _port;

        private Server(int port)
        {
            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port));

            _port = port;
        }

        public Server(IPAddress address, int port) : this(port)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public Server(IPEndPoint endpoint) : this(endpoint?.Port ?? throw new ArgumentNullException(nameof(endpoint)))
        {
            _address = endpoint.Address;
        }

        public Server(DnsEndPoint endpoint) : this(endpoint?.Port ?? throw new ArgumentNullException(nameof(endpoint)))
        {
            _hostName = endpoint.Host;
        }

        public Server(Uri uri)
        {
            if (!uri?.IsAbsoluteUri ?? throw new ArgumentNullException(nameof(uri)))
                throw new ArgumentException("The provided URI is not absolute");

            _hostName = uri.Host;
            _port = uri.Port;
        }

        public Server(string host, int port) : this(port)
        {
            _hostName = string.IsNullOrEmpty(host) ? throw new ArgumentNullException(nameof(host)) : host;
        }

        /// <summary>
        /// Gets whether this endpoint is represented by a IP address
        /// </summary>
        public bool IsEndPoint => _address != null;

        /// <summary>
        /// Gets whether this server is represented by a host name
        /// </summary>
        public bool IsUri => _hostName != null;

        /// <summary>
        /// Gets this server's port
        /// </summary>
        public int Port => _port;

        /// <summary>
        /// Gets this server's host name
        /// </summary>
        public string Host => _hostName ?? _address.ToString();

        public override bool Equals(object obj)
        {
            switch(obj)
            {
                case Server server:
                    return Equals(server);
                case DnsEndPoint dns:
                    return dns.Host == _hostName && dns.Port == _port;
                case IPEndPoint endpoint:
                    return endpoint.Address == _address && endpoint.Port == _port;
                default:
                    return false;
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            return IsEndPoint ? new IPEndPoint(_address, _port) : null;
        }

        public DnsEndPoint GetDnsEndPoint()
        {
            return new DnsEndPoint(Host, Port);
        }

        public Uri GetUri()
        {
            return new UriBuilder()
            {
                Host = Host,
                Port = Port
            }.Uri;
        }

        public override int GetHashCode()
        {
            return ((object)_hostName ?? _address).GetHashCode() * 18 ^ _port.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Host}:{Port}";
        }

        public bool Equals(Server other)
        {
            return _port == other._port && _address == other._address && _hostName == other._hostName;
        }

        public static implicit operator Server(IPEndPoint endpoint)
        {
            return new Server(endpoint);
        }

        public static implicit operator Server(DnsEndPoint endpoint)
        {
            return new Server(endpoint);
        }

        public static explicit operator Server(Uri uri)
        {
            return new Server(uri);
        }

        public static explicit operator IPEndPoint(Server server)
        {
            return server.GetIPEndPoint();
        }

        public static explicit operator DnsEndPoint(Server server)
        {
            return server.GetDnsEndPoint();
        }

        public static explicit operator Uri(Server server)
        {
            return server.GetUri();
        }
    }
}
