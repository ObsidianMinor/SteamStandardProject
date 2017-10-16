namespace Steam.Net
{
    internal interface IEncryptor
    {
        void Decrypt(ref byte[] data);
        void Encrypt(ref byte[] data);
    }
}
