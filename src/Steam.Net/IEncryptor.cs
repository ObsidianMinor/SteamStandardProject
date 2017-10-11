namespace Steam.Net
{
    internal interface IEncryptor
    {
        byte[] Decrypt(byte[] data);
        byte[] Encrypt(byte[] data);
    }
}
