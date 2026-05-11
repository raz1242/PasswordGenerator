using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;
using PasswordManager.Core;

namespace PasswordManager.Server {
    [SupportedOSPlatform("windows")]
    public class EncryptionService : IEncryptionService {

        public byte[] Encrypt(string plainText) {
            if (string.IsNullOrWhiteSpace(plainText))
                return Array.Empty<byte>();

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            // This encrypts the data so only the Server's Windows User can read it
            return ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        }

        public string Decrypt(byte[] encryptedBytes) {
            if (encryptedBytes == null || encryptedBytes.Length == 0)
                return string.Empty;

            try {
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (CryptographicException ex) {
                throw new InvalidOperationException("Database Decryption Failed: The data might be corrupted or was encrypted by a different Windows user.", ex);
            }
        }

        /*
         * ---debugging---
        private async Task<string> Encrypt(string plainText) {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using Aes aes = Aes.Create();
            byte[] key = {
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
                };
            aes.Key = key;

            aes.GenerateIV();
            byte[] iv = aes.IV;

            using var memoryStream = new MemoryStream();

            await memoryStream.WriteAsync(iv, 0, iv.Length).ConfigureAwait(false);
            using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                using (var streamWriter = new StreamWriter(cryptoStream)) {
                    await streamWriter.WriteAsync(plainText).ConfigureAwait(false);
                }
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        private async Task<string> Decrypt(string encryptedData) {
            if (string.IsNullOrEmpty(encryptedData))
                return encryptedData;

            byte[] fullCipher = Convert.FromBase64String(encryptedData);

            using Aes aes = Aes.Create();
            byte[] key = {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
            };
            aes.Key = key;

            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);

            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var memoryStream = new MemoryStream(cipher);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return await streamReader.ReadToEndAsync().ConfigureAwait(false);
        }*/
    }
}