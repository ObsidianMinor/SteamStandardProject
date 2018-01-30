using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Steam.KeyValues
{
    public sealed class KeyValueParserConfig
    {
        private readonly static string[] WindowsCondition = new[] { "$WIN32", "$WINDOWS" };

        private readonly static string[] MacCondition = new[] { "$POSIX", "$OSX", "$WIN32" };

        private readonly static string[] LinuxCondition = new[] { "$POSIX", "$LINUX", "$WIN32" };

        /// <summary>
        /// Sets the conditions to evaluate with. Uses OS specific conditions by default
        /// </summary>
        public string[] Conditions { get; set; } = GetDefaultConditions();

        /// <summary>
        /// Sets the memory pool this parser will use
        /// </summary>
        public MemoryPool<byte> Pool { get; set; }

        /// <summary>
        /// Represents no conditions
        /// </summary>
        public static readonly string[] NoConditions = new string[0];

        /// <summary>
        /// Gets the default conditions for the current OS type
        /// </summary>
        /// <returns></returns>
        public static string[] GetDefaultConditions()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return WindowsCondition;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return MacCondition;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return LinuxCondition;
            else
                return NoConditions;
        }
    }
}
