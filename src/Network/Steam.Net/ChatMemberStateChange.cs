using System;

namespace Steam.Net
{
    [Flags]
    public enum ChatMemberStateChange
    {
        Entered = 0x01,
        Left = 0x02,
        Disconnected = 0x04,
        Kicked = 0x08,
        Banned = 0x10,
        VoiceSpeaking = 0x1000,
        VoiceDoneSpeaking = 0x2000,
    }
}
