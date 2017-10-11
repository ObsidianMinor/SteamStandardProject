using System.Runtime.InteropServices;

namespace Steam.Net.Messages.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct JoinChat
    {
        internal ulong ChatId;
        internal byte IsVoiceSpeaker;
    }
}
