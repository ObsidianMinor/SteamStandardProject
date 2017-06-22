using Steam.Web.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Steam.Net
{
    internal class ServerList
    {
        uint _currentCellId;
        SteamWebApiClient _web;
        Dictionary<Uri, bool> _currentWebSockets = new Dictionary<Uri, bool>();
        Dictionary<IPEndPoint, bool> _currentEndpoints = new Dictionary<IPEndPoint, bool>();
        IEnumerator<KeyValuePair<IPEndPoint, bool>> _currentEndpointsEnumerator;
        IEnumerator<KeyValuePair<Uri, bool>> _currentWebSocketsEnumerator;

        internal bool HasValidWebSocketManagers => _currentWebSockets.Count(u => u.Value) != 0;
        internal bool HasValidManagers => _currentEndpoints.Count(u => u.Value) != 0;

        internal ServerList(uint cellId, SteamWebApiClient web, IEnumerable<Uri> providedWebSockets, IEnumerable<IPEndPoint> providedEndpoints)
        {
            _currentCellId = cellId;
            _web = web;
            if (providedWebSockets.Count() != 0)
            {
                _currentWebSockets = providedWebSockets.ToDictionary(u => u, u => true);
                _currentWebSocketsEnumerator = _currentWebSockets.GetEnumerator();
            }
            if (providedEndpoints.Count() != 0)
            {
                _currentEndpoints = providedEndpoints.ToDictionary(e => e, e => true);
                _currentEndpointsEnumerator = _currentEndpoints.GetEnumerator();
            }
        }

        internal async Task<IPEndPoint> GetCurrentConnectionManagerAsync()
        {
            if (!HasValidManagers)
            {
                var endpoints = await _web.GetConnectionManagerList(_currentCellId, null, null);
                _currentEndpoints = endpoints.ToDictionary(e => e, e => true); // wot
                _currentEndpointsEnumerator = _currentEndpoints.GetEnumerator();
            }

            while(!_currentEndpointsEnumerator.Current.Value)
                _currentEndpointsEnumerator.MoveNext();

            return _currentEndpointsEnumerator.Current.Key;
        }

        internal void MarkCurrent()
        {
            _currentEndpoints[_currentEndpointsEnumerator.Current.Key] = false;
            _currentEndpointsEnumerator = _currentEndpoints.GetEnumerator();
        }

        internal void MarkCurrentWebSocket()
        {
            _currentWebSockets[_currentWebSocketsEnumerator.Current.Key] = false;
            _currentWebSocketsEnumerator = _currentWebSockets.GetEnumerator();
        }

        internal async Task<Uri> GetCurrentWebSocketConnectionManagerAsync()
        {
            if (!HasValidWebSocketManagers)
            {
                var endpoints = await _web.GetWebSocketConnectionManagerList(_currentCellId, null, null);
                _currentWebSockets = endpoints.ToDictionary(e => 
                {
                    string[] split = e.Split(new[] { ':' }, 2);
                    return new UriBuilder("wss", split[0], int.Parse(split[1]), "/cmsocket/").Uri;
                }, e => true); // wot
                _currentWebSocketsEnumerator = _currentWebSockets.GetEnumerator();
            }

            while (!_currentWebSocketsEnumerator.Current.Value)
                _currentWebSocketsEnumerator.MoveNext();

            return _currentWebSocketsEnumerator.Current.Key;
        }
    }
}
