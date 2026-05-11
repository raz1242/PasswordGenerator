using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Core {
    public interface IEncryptionService {
        byte[] Encrypt(string plainText);
        string Decrypt(byte[] encryptedBytes);
    }
}
