namespace Steam.Net
{
    internal class NoEncryptor : IEncryptor
    {
        public void Decrypt(ref byte[] data)
        {
            return;
        }

        public void Encrypt(ref byte[] data)
        {
            return;
        }
    }
}
