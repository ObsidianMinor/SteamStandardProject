namespace Steam.Rest
{
    /// <summary>
    /// The HTTP methods supported by Steam
    /// </summary>
    public enum HttpMethod
    {
        /// <summary>
        /// The GET method
        /// </summary>
        Get = 1, // set to 1 for Steam.API
        /// <summary>
        /// The HEAD method
        /// </summary>
        Head,
        /// <summary>
        /// The POST method
        /// </summary>
        Post,
        /// <summary>
        /// The PUT method
        /// </summary>
        Put,
        /// <summary>
        /// The DELETE method
        /// </summary>
        Delete,
        /// <summary>
        /// The OPTIONS method
        /// </summary>
        Options,
        /// <summary>
        /// The PATCH method
        /// </summary>
        Patch
    }
}
