using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Steam.Local.Apps
{
    /// <summary>
    /// Represents a Steam library
    /// </summary>
    public class Library
    {
        public string DirectoryPath { get; }

        private List<LocalApp> _apps;
        public IReadOnlyCollection<LocalApp> Apps { get => _apps.AsReadOnly(); }

        public static Library CreateFromDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                return null;
            
            return new Library(directory, Directory.EnumerateFiles(directory, "appmanifest_*.acf", SearchOption.TopDirectoryOnly).Select(file => LocalApp.CreateFromFile(file)));
        }

        private Library(string directory, IEnumerable<LocalApp> apps)
        {
            DirectoryPath = directory;
            _apps = apps.Where(a => a != null).ToList();
        }

        public ulong CalculateUsedSpace()
        {
            ulong total = 0;
            foreach (ulong size in Apps.Select(a => a.SizeOnDisk))
                total += size;

            return total;
        }
    }
}
