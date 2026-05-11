using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics; // Required for Debug.WriteLine

namespace PasswordManager.Core;
public static class NetworkTransit {

    public static string EncryptPayload(string plainText) {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        byte[] key = NetworkKeyManager.GetKey();

        using Aes aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream)) {
            streamWriter.Write(plainText);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string DecryptPayload(string encryptedBase64) {
        if (string.IsNullOrEmpty(encryptedBase64))
            return encryptedBase64;

        try {
            byte[] fullCipher = Convert.FromBase64String(encryptedBase64);

            // Ensure the payload is at least as large as the IV (16 bytes)
            if (fullCipher.Length < 16) {
                Console.WriteLine("Error: Payload is too short to contain an IV.");
                return null;
            }

            byte[] key = NetworkKeyManager.GetKey();

            using Aes aes = Aes.Create();
            aes.Key = key;

            // Note: If the encrypting side didn't use CBC/PKCS7, you must define it here.
            // aes.Mode = CipherMode.CBC; 
            // aes.Padding = PaddingMode.PKCS7;

            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - 16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
            Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var memoryStream = new MemoryStream(cipher);
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (CryptographicException ex) {
            Debug.WriteLine($"Crypto Error (Wrong Key, IV, or Padding): {ex.Message}");
            return null;
        }
        catch (FormatException ex) {
            Debug.WriteLine($"Base64 Formatting Error: {ex.Message}");
            return null;
        }
        catch (Exception ex) {
            Debug.WriteLine($"Unexpected Error: {ex.Message}");
            return null;
        }
    }
}