using System;

namespace Steam.Local.Apps
{
    /// <summary>
    /// List of possible Steam installed app states
    /// </summary>
    [Flags]
    public enum AppState
    {
        Invalid = 0,
        Uninstalled = 1,
        UpdateRequired = 2,
        FullyInstalled = 4,
        [Obsolete("The Encrypted bit field is depricated")] Encrypted = 8,
        UpdateOptional = 16,
        FilesMissing = 32,
        SharedOnly = 64,
        FilesCorrupt = 128,
        UpdateRunning = 256,
        UpdatePaused = 512,
        UpdateStarted = 1024,
        Uninstalling = 2048,
        BackupRunning = 4096,
        AppRunning = 8192,
        ComponentInUse = 16384,
        MovingFolder = 32768
    }
}
