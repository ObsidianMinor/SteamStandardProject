using Steam.Local.Apps;
using System;

namespace Steam.Local
{
    /// <summary>
    /// Represents a Steam installation on a local or remote computer
    /// </summary>
    public class SteamInstallation
    {
        internal SteamFileManager FileManager { get; }

        /// <summary>
        /// Gets path to this Steam installation
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets this Steam installation's corresponding registry
        /// </summary>
        public Registry Registry { get; private set; }

        /// <summary>
        /// Gets all the libraries in this Steam installation
        /// </summary>
        public LibraryCollection Libraries { get; private set; }

        private SteamInstallation(string path)
        {
            Path = path;
            FileManager = new SteamFileManager(this);
        }

        /// <summary>
        /// Updates all data in this <see cref="SteamInstallation"/> with data from the disk
        /// </summary>
        public void Update()
        {
            throw new NotImplementedException();
        }

        public static SteamInstallation LoadFromRegistry(Registry registry)
        {
            SteamInstallation install = new SteamInstallation(registry.InstallPath)
            {
                Registry = registry
            };
            install.Update();
            return install;
        }

        public static SteamInstallation GetInstallation() => LoadFromRegistry(Registry.GetRegistry());

        /// <summary>
        /// Gets a <see cref="SteamInstallation"/> from the specified path. This does not load a coresponding <see cref="Local.Registry"/>
        /// </summary>
        /// <param name="path">The path to a Steam installation</param>
        /// <returns>A new Steam installation</returns>
        /// <remarks>
        /// This is used for the LoadFromRegistry method to load a Steam installation using the provided path in the registry.
        /// It can also be used to manually load an installation. For example: a remote installation or a SteamCMD instance
        /// </remarks>
        public static SteamInstallation GetInstallationFromPath(string path)
        {
            SteamInstallation install = new SteamInstallation(path);
            install.Update();
            return install;
        }
    }
}
