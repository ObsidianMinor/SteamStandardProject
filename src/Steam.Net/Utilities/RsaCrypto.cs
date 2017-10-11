using System;
using System.Security.Cryptography;

namespace Steam.Net.Utilities
{
    internal class RsaCrypto : IDisposable
    {
        readonly RSA _rsa;

        internal RsaCrypto(byte[] key)
        {
            AsnKeyParser keyParse = new AsnKeyParser(key);

            _rsa = RSA.Create();
            _rsa.ImportParameters(keyParse.ParseRSAPublicKey());
        }

        public byte[] Encrypt(byte[] input)
        {
            return _rsa.Encrypt(input, RSAEncryptionPadding.OaepSHA1);
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }
}
