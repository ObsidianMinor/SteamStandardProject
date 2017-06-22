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

        public byte[] Decrypt(byte[] data)
        {
            return CryptoUtils.SymmetricDecrypt(data, _sessionKey, out _);
        }

        public byte[] Encrypt(byte[] data)
        {
            return CryptoUtils.SymmetricEncrypt(data, _sessionKey);
        }
    }
}
