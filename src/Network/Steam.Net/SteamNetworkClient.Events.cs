using Steam.Common;
using Steam.Net.Messages.Protobufs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        /// <summary>
        /// The connection is encrypted and messages can now be sent to the servers
        /// </summary>
        public event EventHandler Connected
        {
            add => ApiClient.ConnectedEvent += value;
            remove => ApiClient.ConnectedEvent -= value;
        }

        /// <summary>
        /// The connection is encrypted and a previous login can't be resumed
        /// </summary>
        public event EventHandler CanLogin
        {
            add => ApiClient.CanLoginEvent += value;
            remove => ApiClient.CanLoginEvent -= value;
        }

        /// <summary>
        /// The client has been disconnected from the connection manager
        /// </summary>
        public event EventHandler<Exception> Disconnected
        {
            add => ApiClient.DisconnectedEvent += value;
            remove => ApiClient.DisconnectedEvent -= value;
        }
        
        #region Steam apps
        
        public event EventHandler<IReadOnlyCollection<uint>> VacStatusModified
        {
            add => ApiClient.VacStatusModifiedEvent += value;
            remove => ApiClient.VacStatusModifiedEvent -= value;
        }
        
        public event EventHandler<GameConnectTokens> GameConnectTokensReceived // todo: change GameConnectTokens to something else
        {
            add => ApiClient.GameConnectTokensReceivedEvent += value;
            remove => ApiClient.GameConnectTokensReceivedEvent -= value;
        }

        #endregion

        #region Steam cloud



        #endregion

        #region Steam friends



        #endregion

        #region Steam Game Coordinator



        #endregion

        #region Steam Game Server



        #endregion

        #region Steam Master Server



        #endregion

        #region Steam Screenshots



        #endregion

        #region Steam Trading



        #endregion

        #region Steam Unified Messages



        #endregion

        #region Steam User
        
        public event EventHandler<string> LoginKeyReceived
        {
            add => ApiClient.LoginKeyReceivedEvent += value;
            remove => ApiClient.LoginKeyReceivedEvent -= value;
        }

        public event EventHandler<Result> LoggedOff
        {
            add => ApiClient.LoggedOffEvent += value;
            remove => ApiClient.LoggedOffEvent -= value;
        }

        /// <summary>
        /// Invoked when login was denied and can be continued with a auth or two factor code in <see cref="ContinueLoginAsync(string)"/>
        /// </summary>
        public event EventHandler<LogonResponse> LoginActionRequested
        {
            add => ApiClient.LoginActionRequestedEvent += value;
            remove => ApiClient.LoginActionRequestedEvent -= value;
        }

        /// <summary>
        /// Invoked when login was denied and can't be continued with <see cref="ContinueLoginAsync(string)"/>
        /// </summary>
        public event EventHandler<LogonResponse> LoginRejected
        {
            add => ApiClient.LoginRejectedEvent += value;
            remove => ApiClient.LoginRejectedEvent -= value;
        }

        public event EventHandler LoggedOn
        {
            add => ApiClient.LoggedInEvent += value;
            remove => ApiClient.LoggedInEvent -= value;
        }

        #endregion

        #region Steam User Stats



        #endregion

        #region Steam Workshop



        #endregion

    }
}
