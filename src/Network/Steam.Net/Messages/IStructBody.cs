namespace Steam.Net.Messages
{
    internal interface IStructBody<T> where T : new()
    {
        T Body { get; set; }
    }
}
