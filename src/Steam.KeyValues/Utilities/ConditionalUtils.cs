using System;
using System.Collections.Generic;

namespace Steam.KeyValues.Utilities
{
    internal static class ConditionalUtils
    {
        internal static IEnumerable<Conditional> Seperate(this Conditional conditional)
        {
            
        }

        internal static bool IsNot(this Conditional conditional)
        {
            switch (conditional)
            {
                case Conditional.NotLinux:
                case Conditional.NotMac:
                case Conditional.NotPC:
                case Conditional.NotPosix:
                case Conditional.NotWindows:
                case Conditional.NotXbox360:
                    return true;
                default:
                    return false;
            }
        }

        internal static string ToOriginalString(this Conditional conditional)
        {
            
        }

        internal static string ToString(this Conditional conditional)
        {
            switch (conditional)
            {
                case Conditional.Linux:
                    return "LINUX";
                case Conditional.MacOS:
                    return "OSX";
                case Conditional.PC:
                    return "WIN32";
                case Conditional.Windows:
                    return "WINDOWS";
                case Conditional.Posix:
                    return "POSIX";
                case Conditional.Xbox360:
                    return "X360";
                case Conditional.None:
                    return string.Empty;
                case Conditional.NotLinux:
                case Conditional.NotMac:
                case Conditional.NotPosix:
                case Conditional.NotWindows:
                    return conditional.ToOriginalString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(conditional));
            }
        }
    }
}
