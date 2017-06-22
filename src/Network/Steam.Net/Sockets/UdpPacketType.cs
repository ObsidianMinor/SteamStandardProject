namespace Steam.Net.Sockets
{
    internal enum UdpPacketType : byte
    {
        Invalid = 0,
        ChallengeRequest = 1,
        Challenge = 2,
        Connect = 3,
        Accept = 4,
        Disconnect = 5,
        Data = 6,
        Datagram = 7,
        Max = 8,
    }
}
