using System;

namespace Steam.Local
{
    internal class UnixRegistry : Registry
    {
        public override string InstallPath { get; set; }
        public override string Language { get; set; }
        public override string TempAppCommandLine { get; set; }
        public override string TempAppPath { get; set; }
        public override int SteamProcessId { get; set; }
        public override bool AlreadyRetriedOfflineMode { get; set; }
        public override string AutoLoginUser { get; set; }
        public override bool BigPictureInForeground { get; set; }
        public override bool DirectWriteEnabled { get; set; }
        public override string LastGameNameUsed { get; set; }
        public override string PseudoUUID { get; set; }
        public override int Rate { get; set; }
        public override bool RememberPassword { get; set; }
        public override bool Restart { get; set; }
        public override string CurrentSkin { get; set; }
        public override string SourceModInstallPath { get; set; }
        public override bool StartupMode { get; set; }
        public override string SteamExe { get; set; }
        public override string SteamInstaller { get; set; }
        public override string SteamPath { get; set; }
        public override bool SuppressAutoRun { get; set; }
        public override int WebHelperFirewall { get; set; }

        internal static UnixRegistry LoadFromFile(string path)
        {
            throw new NotImplementedException();
        }
    }
}
