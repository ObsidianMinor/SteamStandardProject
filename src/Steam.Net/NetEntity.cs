using Steam.Net.Messages;
using Steam.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Net
{
    public abstract class NetEntity<T> : WebEntity<T> where T : SteamNetworkClient
    {
        protected NetEntity(T client) : base(client)
        {
        }

        protected Task SendAsync(byte[] data) => Client.SendAsync(data);
        protected Task SendAsync(NetworkMessage message) => Client.SendAsync(message);
        protected Task<NetworkMessage> SendJobAsync(NetworkMessage message) => Client.SendJobAsync(message);
        protected Task<T> SendJobAsync<T>(NetworkMessage message) => Client.SendJobAsync<T>(message);
        protected Task<TResult> SendJobAsync<TResponse, TResult>(NetworkMessage message, Func<TResponse, bool> completionFunc, Func<IEnumerable<TResponse>, TResult> combinationFunc) 
            => Client.SendJobAsync<TResponse, TResult>(message, completionFunc, combinationFunc);
    }
}
