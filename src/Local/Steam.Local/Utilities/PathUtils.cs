using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Steam.Local.Utilities
{
    internal class PathUtils
    {
        internal static bool Equals(string path1, string path2)
        {
            return Path.GetFullPath(path1).Equals(Path.GetFullPath(path2));
        }
    }
}
