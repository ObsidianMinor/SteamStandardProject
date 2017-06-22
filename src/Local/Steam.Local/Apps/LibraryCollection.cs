using Steam.KeyValues;
using Steam.Local.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Steam.Local.Apps
{
    public class LibraryCollection
    {
        readonly SteamInstallation _steam;
        
        public DateTimeOffset TimeNextStatsReport { get; set; }
        public long ContentStatsID { get; set; }

        public List<Library> ExternalLibraries { get; set; }

        public Library RootLibrary { get; private set; }

        /// <summary>
        /// Creates a Steam library at the specified directory. If a library already exists, it returns that library
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Library CreateLibrary(string path)
        {
            if (Contains(path))
                return ExternalLibraries.FirstOrDefault(l => PathUtils.Equals(l.DirectoryPath, path));

            DirectoryInfo directory = Directory.Exists(path) ? new DirectoryInfo(path) : Directory.CreateDirectory(path);
            string steamDll = Path.Combine(directory.FullName, "steam.dll");
            if (!File.Exists(steamDll))
            {
                try
                {
                    File.Copy(Path.Combine(_steam.Path, "Steam.dll"), steamDll);
                }
                catch (FileNotFoundException) { } // We don't need to worry about it, when steam starts it'll copy steam.dll over anyway. 
                                                  // We're just being nice and moving it over so doesn't have to
            }
            DirectoryInfo steamapps = directory.CreateSubdirectory("steamapps");
            steamapps.CreateSubdirectory("common");
            steamapps.CreateSubdirectory("downloading");
            steamapps.CreateSubdirectory("temp");
            steamapps.CreateSubdirectory("workshop");
            Library lib = Library.CreateFromDirectory(path);
            ExternalLibraries.Add(lib);
            return lib;
        }

        public bool Contains(string libraryPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves this library collection to its original file
        /// </summary>
        public void Save()
        {

        }

        private LibraryCollection(SteamInstallation install)
        {
            _steam = install;
        }
    }
}
