using System.Security.Cryptography;
using System.IO;
using System;

namespace PasswordManager.Core;
public static class NetworkKeyManager {
    private const string KeyFilePath = "network.key";
    private const string KeyTextFilePath = "PAIRING_CODE";
    private static byte[]? _currentKey;

    public static bool SetManualKey(string base64Key) {
        try {
            if (string.IsNullOrWhiteSpace(base64Key)) 
                return false;

            _currentKey = Convert.FromBase64String(base64Key);
            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    public static byte[] GetKey() {
        if (_currentKey != null) 
            return _currentKey;

        if (File.Exists(KeyFilePath)) {
            // Key file exists, read and decrypt it
            byte[] encryptedKey = File.ReadAllBytes(KeyFilePath);
            _currentKey = ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser);
            if (!File.Exists(KeyTextFilePath)) {
                string base64Key = Convert.ToBase64String(_currentKey);
                File.WriteAllText(KeyTextFilePath, base64Key);
            }
            return _currentKey;
        }
        else {
            // No key file, generate a new key, encrypt and save it
            _currentKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(_currentKey);
            }

            byte[] encryptedKey = ProtectedData.Protect(_currentKey, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(KeyFilePath, encryptedKey);

            string base64Key = Convert.ToBase64String(_currentKey);

            Console.WriteLine("\n=======================================================");
            Console.WriteLine("--NEW NETWORK KEY GENERATED!--");
            Console.WriteLine("You must enter this key into your Phone/Desktop App to pair it:");
            Console.WriteLine(base64Key);
            Console.WriteLine("=======================================================\n");

            File.WriteAllText(KeyTextFilePath, base64Key);

            return _currentKey;
        }
    }
}