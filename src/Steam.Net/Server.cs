using System;
using System.Net;

namespace Steam.Net
{
    /// <summary>
    /// Represents a Steam server that is either a URI or an IP endpoint
    /// </summary>
    public class Server : IEquatable<Server>
    {
        private readonly IPEndPoint _endpoint;
        private readonly Uri _uri;

        public Server(IPAddress address, int port)
        {
            _endpoint = new IPEndPoint(address ?? throw new ArgumentNullException(nameof(address)), port);
        }

        public Server(IPEndPoint endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            _endpoint = new IPEndPoint(endpoint.Address, endpoint.Port);
        }

        public Server(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The provided URI is not absolute");

            _uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public bool IsEndPoint => _endpoint != null;
        public bool IsUri => _uri != null;

        public bool TryGetIPEndPoint(out IPEndPoint endpoint)
        {
            if (_endpoint != null)
            {
                endpoint = new IPEndPoint(_endpoint.Address, _endpoint.Port);
                return true;
            }
            else if (_uri.IsAbsoluteUri && _uri.HostNameType == UriHostNameType.IPv4 && _uri.HostNameType == UriHostNameType.IPv6 && IPAddress.TryParse(_uri.Host, out var address))
            {
                endpoint = new IPEndPoint(address, _uri.Port);
                return true;
            }
            else
            {
                endpoint = null;
                return false;
            }
        }

        public IPEndPoint GetIPEndPoint()
        {
            if (!TryGetIPEndPoint(out var endpoint))
                throw new InvalidOperationException("Could not convert Server to IPEndPoint");
            else
                return endpoint;
        }

        public Uri GetUri()
        {
            if (_endpoint != null)
            {
                return new UriBuilder()
                {
                    Host = _endpoint.Address.ToString(),
                    Port = _endpoint.Port
                }.Uri;
            }
            else
            {
                return _uri;
            }
        }

        public static implicit operator Server(IPEndPoint endpoint)
        {
            return new Server(endpoint);
        }

        public static implicit operator Server(Uri uri)
        {
            return new Server(uri);
        }

        public static implicit operator Uri(Server server) => server.GetUri();

        public static explicit operator IPEndPoint(Server server) => server.GetIPEndPoint();

        public override bool Equals(object obj)
        {
            if (obj is Server server)
                return Equals(server);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return _endpoint?.GetHashCode() ?? _uri.GetHashCode();
        }

        public override string ToString()
        {
            return _endpoint?.ToString() ?? _uri.ToString();
        }

        public bool Equals(Server other)
        {
            return _endpoint == other._endpoint && _uri == other._uri;
        }
    }
}
