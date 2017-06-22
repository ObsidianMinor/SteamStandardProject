using Microsoft.Win32;
using System.Linq;
using System.Runtime.InteropServices;

namespace Steam.Local
{
    internal class WindowsRegistry : Registry
    {
        private RegistryKey _localMachineHive;
        private RegistryKey _currentUserHive;

        public override string InstallPath
        {
            get => GetStringValue(true, "InstallPath");
            set => SetValue(true, value, "InstallPath");
        }

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

        internal WindowsRegistry()
        {
            _localMachineHive = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($"SOFTWARE\\{(RuntimeInformation.OSArchitecture == Architecture.X64 ? "WOW6432Node\\" : "")}Valve\\Steam");
            _currentUserHive = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Valve\\Steam");
        }

        private int GetDwordValue(bool localMachine, params string[] subKeys)
        {
            RegistryKey subKey = GetSubkeyButLast(localMachine, subKeys);
            return (int)subKey.GetValue(subKeys.Last());
        }

        private bool GetBoolValue(bool localMachine, params string[] subKeys) => GetDwordValue(localMachine, subKeys) != 0;

        private string GetStringValue(bool localMachine, params string[] subKeys)
        {
            RegistryKey subKey = GetSubkeyButLast(localMachine, subKeys);
            return subKey.GetValue(subKeys.Last()).ToString();
        }

        private void SetValue(bool localMachine, object value, params string[] subKeys)
        {
            RegistryKey subKey = GetSubkeyButLast(localMachine, subKeys);
            subKey.SetValue(subKeys.Last(), value);
        }

        private void SetBoolValue(bool localMachine, bool value, params string[] subKeys) => SetValue(localMachine, value ? 1 : 0, subKeys);

        private RegistryKey GetSubkeyButLast(bool localMachine, string[] subKeys)
        {
            RegistryKey subKey = localMachine ? _localMachineHive : _currentUserHive;
            for (int i = 0; i < subKeys.Length - 1; i++)
                subKey = subKey.OpenSubKey(subKeys[i]);

            return subKey;
        }
    }
}
