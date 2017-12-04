using System.Runtime.InteropServices;

namespace Steam.KeyValues
{
    static class KeyValueConstants
    {
        public const byte OpenBrace = (byte)'{';
        public const byte CloseBrace = (byte)'}';
        public const byte OpenBracket = (byte)'[';
        public const byte CloseBracket = (byte)']';
        public const byte Space = (byte)' ';
        public const byte Tab = (byte)'\t';
        public const byte CarriageReturn = (byte)'\r';
        public const byte LineFeed = (byte)'\n';
        public const byte Bang = (byte)'!'; // it's easier to write Bang than ExclamationMark
        public const byte Quote = (byte)'"';
        public const byte BackSlash = (byte)'\\'; // I'M REALLY FEELING IT
        
        public static readonly byte[] Include = { (byte)'#', (byte)'i', (byte)'n', (byte)'c', (byte)'l', (byte)'u', (byte)'d', (byte)'e' };
        public static readonly byte[] Base = { (byte)'#', (byte)'b', (byte)'a', (byte)'s', (byte)'e' };
        public static readonly byte[] X360 = { (byte)'$', (byte)'X', (byte)'3', (byte)'6', (byte)'0' };
        public static readonly byte[] WIN32 = { (byte)'$', (byte)'W', (byte)'I', (byte)'N', (byte)'3', (byte)'2' };
        public static readonly byte[] WINDOWS = { (byte)'$', (byte)'W', (byte)'I', (byte)'N', (byte)'D', (byte)'O', (byte)'W', (byte)'S' };
        public static readonly byte[] OSX = { (byte)'$', (byte)'O', (byte)'S', (byte)'X' };
        public static readonly byte[] LINUX = { (byte)'$', (byte)'L', (byte)'I', (byte)'N', (byte)'U', (byte)'X' };
        public static readonly byte[] POSIX = { (byte)'$', (byte)'P', (byte)'O', (byte)'S', (byte)'I', (byte)'X', };

        public static readonly bool IsWindows;
        public static readonly bool IsMac;
        public static readonly bool IsLinux;
        public static readonly bool IsPosix;
        public const bool IsXbox = false;
        public const bool IsPC = true; // until the .NET Standard is on Xbox 360 these are constants. wake me up when that happens

        static KeyValueConstants()
        {
            IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            IsPosix = IsMac || IsLinux;
        }
    }
}
