namespace Steam.Net
{
    public class PicsProductInfo
    {
        public uint Id { get; private set; }

        public uint ChangeNumber { get; private set; }

        public bool MissingToken { get; private set; }

        public byte[] ShaHash { get; private set; }

        public AppInfo AppInfo { get; private set; }
    }
}