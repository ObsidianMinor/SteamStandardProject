using Steam.Net.Utilities;
using System;

namespace Steam.Net
{
    internal class HmacEncryptor : IEncryptor
    {
        readonly byte[] _sessionKey;
        readonly byte[] _hmacSecret;

        public HmacEncryptor(byte[] sessionKey)
        {
            if (sessionKey.Length != 32)
                throw new InvalidOperationException("Session key must be 32 bytes");

            _sessionKey = sessionKey;
            _hmacSecret = new byte[16];
            Array.Copy(_sessionKey, _hmacSecret, _hmacSecret.Length);
        }

        public byte[] Decrypt(byte[] data)
        {
            return CryptoUtils.SymmetricDecryptWithHmacIv(data, _sessionKey, _hmacSecret);
        }

        public byte[] Encrypt(byte[] data)
        {
            return CryptoUtils.SymmetricEncryptWithHmacIv(data, _sessionKey, _hmacSecret);
        }
    }
}
