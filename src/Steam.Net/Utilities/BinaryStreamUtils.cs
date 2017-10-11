using System;
using System.IO;

namespace Steam.Net.Utilities
{
    internal static class BinaryStreamUtils
    {
        internal static byte[] ReadToEnd(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            long count = reader.BaseStream.Length - reader.BaseStream.Position;
            if (count > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(count), "The resulting stream would be beyond the max value of Int32");

            return reader.ReadBytes((int)count);
        }
    }
}
