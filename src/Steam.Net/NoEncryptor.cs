namespace Steam.Net
{
    internal class NoEncryptor : IEncryptor
    {
        public byte[] Decrypt(byte[] data)
        {
            return data;
        }

        public byte[] Encrypt(byte[] data)
        {
            return data;
        }
    }
}
