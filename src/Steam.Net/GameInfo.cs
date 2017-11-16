using System.Collections.Immutable;
using System.Net;
using Steam.Net.Messages.Protobufs;
using Steam.Net.Utilities;

namespace Steam.Net
{
    public struct GameInfo
    {
        private uint _appId;
        private uint _gameServerIp;
        private uint _serverPort;
        private uint _queryPort;
        private SteamId _steamIdSource;
        private string _gameName;
        private GameId _gameId;
        private ImmutableArray<byte> _gameDataBlob;
        
        /// <summary>
        /// Gets the app ID for this game
        /// </summary>
        public long AppId => _appId;

        /// <summary>
        /// Gets the server IP
        /// </summary>
        public IPEndPoint ServerIp => new IPEndPoint(_gameServerIp.ToIPAddress(), (int)_serverPort);

        /// <summary>
        /// Gets the IP endpoint for querying a the game server
        /// </summary>
        public IPEndPoint ServerQueryIp => new IPEndPoint(_gameServerIp.ToIPAddress(), (int)_queryPort);

        /// <summary>
        /// Gets the Steam ID of the Source server
        /// </summary>
        public SteamId SourceSteamId => _steamIdSource;

        /// <summary>
        /// Gets the name of the game
        /// </summary>
        public string Name => _gameName;

        /// <summary>
        /// Gets the <see cref="GameId"/> of this game
        /// </summary>
        public GameId Id => _gameId;

        /// <summary>
        /// Gets the game data blob as an immutable byte array
        /// </summary>
        public ImmutableArray<byte> DataBlob => _gameDataBlob;

        internal static GameInfo Create(CMsgClientPersonaState.Friend friend)
        {
            return new GameInfo
            {
                _appId = friend.game_played_app_id,
                _gameDataBlob = ImmutableArray.Create(friend.game_data_blob),
                _gameId = friend.gameid,
                _gameName = friend.game_name,
                _gameServerIp = friend.game_server_ip,
                _queryPort = friend.query_port,
                _serverPort = friend.game_server_port,
                _steamIdSource = friend.steamid_source
            };
        }
    }
}
