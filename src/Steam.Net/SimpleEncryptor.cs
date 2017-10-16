using Steam.Net.Utilities;

namespace Steam.Net
{
    internal class SimpleEncryptor : IEncryptor
    {
        readonly byte[] _sessionKey;
        
        public SimpleEncryptor(byte[] sessionKey)
        {
            _sessionKey = sessionKey;
        }

        public void Decrypt(ref byte[] data)
        {
            data = CryptoUtils.SymmetricDecrypt(data, _sessionKey, out _);
        }

        public void Encrypt(ref byte[] data)
        {
            data = CryptoUtils.SymmetricEncrypt(data, _sessionKey);
        }
    }
}
