namespace Steam.Net.Messages
{
    internal interface IPayload
    {
        byte[] Payload { get; set; }
    }
}
