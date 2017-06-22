namespace Steam.Net
{
    public class PicsRequest
    {
        /// <summary>
        /// Gets or sets the ID of the app or package in this request
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// Gets or sets the access token for this request
        /// </summary>
        public ulong AccessToken { get; set; }
        /// <summary>
        /// Gets or set whether to get public data only in this request
        /// </summary>
        public bool PublicOnly { get; set; }
    }
}
