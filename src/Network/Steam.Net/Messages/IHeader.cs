namespace Steam.Net.Messages
{
    internal interface IHeader<T> where T : new()
    {
        T Header { get; set; }
    }
}
