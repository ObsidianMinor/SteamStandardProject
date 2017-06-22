using System;
using System.Runtime.InteropServices;

namespace Steam.Local
{
    /// <summary>
    /// A collection of Steam registry keys
    /// </summary>
    public abstract class Registry
    {
        public abstract string InstallPath { get; set; }
        public abstract string Language { get; set; }
        public abstract string TempAppCommandLine { get; set; }
        public abstract string TempAppPath { get; set; }
        public abstract int SteamProcessId { get; set; }
        public abstract bool AlreadyRetriedOfflineMode { get; set; }
        public abstract string AutoLoginUser { get; set; }
        public abstract bool BigPictureInForeground { get; set; }
        public abstract bool DirectWriteEnabled { get; set; }
        public abstract string LastGameNameUsed { get; set; }
        public abstract string PseudoUUID { get; set; }
        public abstract int Rate { get; set; }
        public abstract bool RememberPassword { get; set; }
        public abstract bool Restart { get; set; }
        public abstract string CurrentSkin { get; set; }
        public abstract string SourceModInstallPath { get; set; }
        public abstract bool StartupMode { get; set; }
        public abstract string SteamExe { get; set; }
        public abstract string SteamInstaller { get; set; }
        public abstract string SteamPath { get; set; }
        public abstract bool SuppressAutoRun { get; set; }
        public abstract int WebHelperFirewall { get; set; }

        /// <summary>
        /// Gets the registry on the local computer
        /// </summary>
        /// <returns>A registry object</returns>
        public static Registry GetRegistry()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsRegistry();
            }
            else
            {
                string path = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? $"{Environment.GetEnvironmentVariable("")}" : $""; // todo: get relevant path
                return UnixRegistry.LoadFromFile(path);
            }
        }
    }
}
