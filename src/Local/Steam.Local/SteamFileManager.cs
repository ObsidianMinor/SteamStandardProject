using Steam.Common;
using Steam.Local.Apps;
using System;
using System.IO;
using Steam.KeyValues;

namespace Steam.Local
{
    /// <summary>
    /// Provides a centralized way to load and save file models for Steam config files
    /// </summary>
    internal class SteamFileManager
    {
        internal SteamInstallation SteamInstall { get; }

        internal SteamFileManager(SteamInstallation install)
        {
            SteamInstall = install;
        }

        internal LocalApp LoadApp(string path)
        {
            Models.AppState state = KeyValueConvert.DeserializeObject<Models.AppState>(File.ReadAllText(path));
            return new LocalApp
            {
                AllowOtherDownloadsWhileRunning = state.AllowOtherDownloadsWhileRunning,
                AutoUpdateBehavior = state.AutoUpdateBehavior,
                BuildID = state.BuildID,
                BytesDownloaded = state.BytesDownloaded,
                BytesToDownload = state.BytesToDownload,
                CheckGuid = state.CheckGuid,
                DlcDownloads = state.DlcDownloads,
                FullValidateAfterNextUpdate = state.FullValidateAfterNextUpdate,
                FullValidateBeforeNextUpdate = state.FullValidateBeforeNextUpdate,
                Id = state.Id,
                InstallFolder = state.InstallFolder,
                InstallScripts = state.InstallScripts,
                LastOwner = state.LastOwner,
                LastUpdated = DateTimeOffset.FromUnixTimeMilliseconds(state.LastUpdated),
                MountedDepots = state.MountedDepots,
                Name = state.Name,
                SharedDepots = state.SharedDepots,
                SizeOnDisk = state.SizeOnDisk,
                StagedDepots = state.StagedDepots,
                StateFlags = state.StateFlags,
                Universe = state.Universe,
                UpdateResult = state.UpdateResult,
                UserConfig = state.UserConfig
            };
        }

        internal void SaveApp(LocalApp app, string path)
        {
            throw new NotImplementedException();
        }

        internal LibraryCollection LoadLibraries(string path)
        {
            throw new NotImplementedException();
        }
    }
}
