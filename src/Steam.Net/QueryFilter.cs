using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Steam.Net
{
    public sealed class QueryFilter
    {
        private bool _notEmpty;
        private bool _empty;
        /// <summary>
        /// Returns dedicated servers
        /// </summary>
        public bool Dedicated { get; set; }
        /// <summary>
        /// Returns VAC secured servers
        /// </summary>
        public bool Secure { get; set; }
        /// <summary>
        /// Returns servers running the specified mod
        /// </summary>
        public string Mod { get; set; }
        /// <summary>
        /// Returns servers running the specified map
        /// </summary>
        public string Map { get; set; }
        /// <summary>
        /// Returns servers running on Linux
        /// </summary>
        public bool Linux { get; set; }
        /// <summary>
        /// Returns servers with no password
        /// </summary>
        public bool NoPassword { get; set; }

        /// <summary>
        /// <para>
        /// Returns servers that are not empty
        /// </para>
        /// <para>
        /// This sets <see cref="Empty"/> to false if it is currently true and the new value is true
        /// </para>
        /// </summary>
        public bool NotEmpty
        {
            get => _notEmpty;
            set
            {
                if (value && _empty)
                    _empty = false;

                _notEmpty = value;
            }
        }
        /// <summary>
        /// Returns servers that are not full
        /// </summary>
        public bool NotFull { get; set; }
        /// <summary>
        /// Returns servers not running the specified app ID
        /// </summary>
        public int NotAppId { get; set; } = -1;
        /// <summary>
        /// Returns servers running the specified app ID
        /// </summary>
        public int AppId { get; set; } = -1;
        /// <summary>
        /// Returns servers that are spectator proxies
        /// </summary>
        public bool IsProxy { get; set; }

        /// <summary>
        /// <para>
        /// Returns servers that are empty
        /// </para>
        /// <para>
        /// This sets <see cref="NotEmpty"/> to false if it is currently true and the new value is true
        /// </para>
        /// </summary>
        public bool Empty
        {
            get => _empty;
            set
            {
                if (value && _notEmpty)
                    _notEmpty = false;

                _empty = value;
            }
        }
        /// <summary>
        /// Returns servers that are whitelisted
        /// </summary>
        public bool Whitelisted { get; set; }
        /// <summary>
        /// Returns servers with all the given tags
        /// </summary>
        public IEnumerable<string> Tags { get; set; }
        /// <summary>
        /// Returns servers with all the specified hidden tags
        /// </summary>
        public IEnumerable<string> HiddenTags { get; set; }
        /// <summary>
        /// Returns servers with any of the specified hidden tags
        /// </summary>
        public IEnumerable<string> AnyHiddenTags { get; set; }
        /// <summary>
        /// Returns servers with their hostname matching the specified hostname
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// Returns servers running the specified version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Returns one server for each unique IP address
        /// </summary>
        public bool UniqueIp { get; set; }
        /// <summary>
        /// Returns servers with the specified IP address
        /// </summary>
        public IPAddress Address { get; set; }
        /// <summary>
        /// Returns servers with the specified <see cref="Address"/> and specified port
        /// </summary>
        public short Port { get; set; } = -1;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("");

            if (Dedicated)
                builder.Append(@"\dedicated\1");

            if (Secure)
                builder.Append(@"\secure\1");

            if (!string.IsNullOrWhiteSpace(Mod))
                builder.Append($@"\gamedir\{Mod}");

            if (!string.IsNullOrWhiteSpace(Map))
                builder.Append($@"\map\{Map}");

            if (Linux)
                builder.Append($@"\linux\1");

            if (NoPassword)
                builder.Append(@"\password\0");

            if (NotEmpty)
                builder.Append(@"\empty\1");

            if (Empty)
                builder.Append(@"\noplayers\1");

            if (NotFull)
                builder.Append(@"\full\1");

            if (IsProxy)
                builder.Append(@"\proxy\1");

            if (AppId > 0)
                builder.Append($@"\appid\{AppId}");

            if (NotAppId > 0)
                builder.Append($@"\napp\{NotAppId}");

            if (Whitelisted)
                builder.Append(@"\white\1"); // that's racist
                                             // don't take that seriously
            if (Tags != null || Tags.Count() != 0)
                builder.Append($@"\gametype\{string.Join(",", Tags)}");

            if (HiddenTags != null || HiddenTags.Count() != 0)
                builder.Append($@"\gamedata\{string.Join(",", HiddenTags)}");

            if (AnyHiddenTags != null || AnyHiddenTags.Count() != 0)
                builder.Append($@"\gamedataor\{string.Join(",", AnyHiddenTags)}");

            if (!string.IsNullOrWhiteSpace(Hostname))
                builder.Append($@"\name_match\{Hostname}");

            if (!string.IsNullOrWhiteSpace(Version))
                builder.Append($@"\version_match\{Version}");

            if (UniqueIp)
                builder.Append(@"\collapse_addr_hash\1");

            if (Address != null)
            {
                builder.Append($@"\gameaddr\");
                if (Port > -1)
                    builder.Append($"{Address}:{Port}");
                else
                    builder.Append(Address);
            }

            return builder.ToString();
        }
    }
}
