using Steam.Net.Messages.Protobufs;
using System;
using System.Collections.Generic;

namespace Steam.Net
{
    public partial class SteamNetworkClient
    {
        /// <summary>
        /// The connection is encrypted and messages can now be sent to the servers
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// The connection is encrypted and a previous login can't be resumed
        /// </summary>
        public event EventHandler CanLogin;

        /// <summary>
        /// The client has been disconnected from the connection manager
        /// </summary>
        public event EventHandler<Exception> Disconnected;
        
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
        
        public event EventHandler<Result> LoggedOff;

        /// <summary>
        /// Invoked when login was denied and can be continued with a auth or two factor code in <see cref="ContinueLoginAsync(string)"/>
        /// </summary>
        public event EventHandler LoginActionRequested;

        /// <summary>
        /// Invoked when login was denied and can't be continued with <see cref="ContinueLoginAsync(string)"/>
        /// </summary>
        public event EventHandler LoginRejected;

        public event EventHandler LoggedOn;

        #endregion

        #region Steam User Stats



        #endregion

        #region Steam Workshop



        #endregion

    }
}
