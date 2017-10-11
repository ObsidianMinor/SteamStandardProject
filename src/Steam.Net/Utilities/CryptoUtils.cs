using Steam.Cryptography;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Steam.Net.Utilities
{
    internal static class CryptoUtils
    {
        internal static byte[] GenerateBytes(int size)
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                byte[] block = new byte[size];
                generator.GetBytes(block);
                return block;
            }
        }

        internal static byte[] SymmetricDecrypt(byte[] data, byte[] key, out byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                byte[] cryptedIv = new byte[16];
                iv = new byte[cryptedIv.Length];
                Array.Copy(data, 0, cryptedIv, 0, cryptedIv.Length);

                byte[] cipherText = new byte[data.Length - cryptedIv.Length];
                Array.Copy(data, cryptedIv.Length, cipherText, 0, cipherText.Length);

                using (ICryptoTransform transform = aes.CreateDecryptor(key, null))
                    iv = transform.TransformFinalBlock(cryptedIv, 0, cryptedIv.Length);

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform transform = aes.CreateDecryptor(key, iv))
                using (MemoryStream stream = new MemoryStream(cipherText))
                using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
                {
                    byte[] plaintext = new byte[cipherText.Length];

                    int length = cryptoStream.Read(plaintext, 0, plaintext.Length);

                    byte[] output = new byte[length];
                    Array.Copy(plaintext, 0, output, 0, length);

                    return output;
                }
            }
        }

        internal static byte[] SymmetricEncryptWithIv(byte[] data, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.BlockSize = 128;
                aes.KeySize = 256;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                byte[] cryptedIv = new byte[16];

                using (ICryptoTransform aesTransform = aes.CreateEncryptor(key, null))
                {
                    cryptedIv = aesTransform.TransformFinalBlock(iv, 0, iv.Length);
                }

                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform aesTransform = aes.CreateEncryptor(key, iv))
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(stream, aesTransform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    byte[] cipherText = stream.ToArray();
                    byte[] output = new byte[cryptedIv.Length + cipherText.Length];

                    Array.Copy(cryptedIv, 0, output, 0, cryptedIv.Length);
                    Array.Copy(cipherText, 0, output, cryptedIv.Length, cipherText.Length);

                    return output;
                }
            }
        }

        internal static byte[] SymmetricEncrypt(byte[] input, byte[] key)
        {
            byte[] iv = GenerateBytes(16);
            return SymmetricEncryptWithIv(input, key, iv);
        }

        internal static byte[] CrcHash(byte[] input)
        {
            using (Crc32 crc = new Crc32())
            {
                byte[] hash = crc.ComputeHash(input);
                Array.Reverse(hash);
                return hash;
            }
        }

        internal static byte[] SymmetricEncryptWithHmacIv(byte[] data, byte[] key, byte[] hmacSecret)
        {
            byte[] iv = new byte[16];
            byte[] random = GenerateBytes(3);
            Array.Copy(random, 0, iv, iv.Length - random.Length, random.Length);

            using (HMACSHA1 hmac = new HMACSHA1(hmacSecret))
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(random, 0, random.Length);
                stream.Write(data, 0, data.Length);
                stream.Seek(0, SeekOrigin.Begin);

                byte[] hash = hmac.ComputeHash(stream);
                Array.Copy(hash, iv, iv.Length - random.Length);
            }

            return SymmetricEncryptWithIv(data, key, iv);
        }

        internal static byte[] SymmetricDecryptWithHmacIv(byte[] data, byte[] key, byte[] hmacSecret)
        {
            byte[] plaintext = SymmetricDecrypt(data, key, out byte[] iv);

            byte[] hmacBytes = null;
            using (HMACSHA1 hmac = new HMACSHA1(hmacSecret))
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(iv, iv.Length - 3, 3);
                stream.Write(plaintext, 0, plaintext.Length);
                stream.Seek(0, SeekOrigin.Begin);

                hmacBytes = hmac.ComputeHash(stream);
            }

            if (!hmacBytes.Take(iv.Length - 3).SequenceEqual(iv.Take(iv.Length - 3)))
                throw new CryptographicException("Unable to decrypt packet: HMAC from server did not match computed HMAC");

            return plaintext;
        }
    }
}
