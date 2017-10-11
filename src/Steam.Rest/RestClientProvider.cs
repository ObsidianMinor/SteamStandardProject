namespace Steam.Rest
{
    /// <summary>
    /// A delegate to create a new <see cref="IRestClient"/>
    /// </summary>
    /// <returns>A new <see cref="IRestClient"/></returns>
    public delegate IRestClient RestClientProvider();
}
