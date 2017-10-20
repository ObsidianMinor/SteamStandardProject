using System;
using System.Threading.Tasks;

namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        private readonly AsyncEvent<Func<Task>> _connected = new AsyncEvent<Func<Task>>();
        /// <summary>
        /// The client is connected to a connection manager
        /// </summary>
        public event Func<Task> Connected
        {
            add => _connected.Add(value);
            remove => _connected.Remove(value);
        }

        private readonly AsyncEvent<Func<Task>> _ready = new AsyncEvent<Func< Task>>();
        /// <summary>
        /// The connection is ready for a login request
        /// </summary>
        public event Func<Task> Ready
        {
            add => _ready.Add(value);
            remove => _ready.Remove(value);
        }
        
        private readonly AsyncEvent<Func<Exception, Task>> _disconnected = new AsyncEvent<Func<Exception, Task>>();
        /// <summary>
        /// The client has been disconnected from the connection manager
        /// </summary>
        public event Func<Exception, Task> Disconnected
        {
            add => _disconnected.Add(value);
            remove => _disconnected.Remove(value);
        }

        #region Steam apps

        #endregion

        #region Steam cloud



        #endregion

        #region Steam friends



        #endregion

        #region Steam Game Server



        #endregion

        #region Steam Screenshots



        #endregion

        #region Steam Trading



        #endregion

        #region Steam Unified Messages



        #endregion

        #region Steam User

        private readonly AsyncEvent<Func<Result, Task>> _loggedOffEvent = new AsyncEvent<Func<Result, Task>>();
        /// <summary>
        /// Invoked when the client is logged off
        /// </summary>
        public event Func<Result, Task> LoggedOff
        {
            add => _loggedOffEvent.Add(value);
            remove => _loggedOffEvent.Remove(value);
        }

        private readonly AsyncEvent<Func<Result, SteamId, bool, Task>> _loginRejectedEvent = new AsyncEvent<Func<Result, SteamId, bool, Task>>();
        /// <summary>
        /// Invoked when login was denied
        /// </summary>
        public event Func<Result, SteamId, bool, Task> LoginRejected
        {
            add => _loginRejectedEvent.Add(value);
            remove => _loginRejectedEvent.Remove(value);
        }

        private readonly AsyncEvent<Func<Task>> _loggedOnEvent = new AsyncEvent<Func<Task>>();
        /// <summary>
        /// Invoked when the client has logged on
        /// </summary>
        public event Func<Task> LoggedOn
        {
            add => _loggedOnEvent.Add(value);
            remove => _loggedOnEvent.Remove(value);
        }

        #endregion

        #region Steam User Stats



        #endregion

        #region Steam Workshop



        #endregion

    }
}
