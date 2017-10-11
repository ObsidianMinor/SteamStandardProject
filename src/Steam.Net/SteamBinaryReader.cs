using System.IO;
using System.Text;

namespace Steam.Net
{
    internal class SteamBinaryReader : BinaryReader
    {
        public SteamBinaryReader(Stream input) : base(input) { }

        public SteamBinaryReader(Stream input, Encoding encoding) : base(input, encoding) { }

        public SteamBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        /// <summary>
        /// Reads a null terminated string
        /// </summary>
        /// <returns></returns>
        public override string ReadString()
        {
            string result = "";
            for (char current = ReadChar(); current != 0; current = ReadChar())
                result += current;
            return result;
        }
    }
}
