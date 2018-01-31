using System.Runtime.InteropServices;

namespace Steam.KeyValues
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct DbRow
    {
        public readonly static int Size = sizeof(DbRow);

        public int KeyLocation;
        public int KeyLength;
        public int Location;
        public int Length;
        public KeyValueType Type;

        public DbRow(int keyIndex, int keyLength, KeyValueType type, int valueIndex, int length)
        {
            KeyLocation = keyIndex;
            KeyLength = keyLength;
            Location = valueIndex;
            Length = length;
            Type = type;
        }

        public bool IsSimpleValue => Type != KeyValueType.None;
    }
}
