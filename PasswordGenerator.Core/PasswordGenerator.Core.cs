using System.Security.Cryptography;

namespace PasswordGenerator.Core;

public enum PasswordType {                                                               // Enum to represent the different types of passwords that can be generated
    AlphanumericSpecial = 1,
    Alphanumeric = 2,
    Numeric = 3
}

public static class GeneratePassword {
    const string NumericChars = "0123456789";
    const string AlphaChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string SpecialChars = "!@#$%^&*()_+";


    /* Method to get the character pool based on the selected password type
     */
    static string GetCharacterPool(PasswordType type) {
        return type switch {
            PasswordType.AlphanumericSpecial => NumericChars + AlphaChars + SpecialChars,
            PasswordType.Alphanumeric => NumericChars + AlphaChars,
            PasswordType.Numeric => NumericChars,
            _ => NumericChars + AlphaChars + SpecialChars
        };
    }

    /* Method to generate a password based on the specified length and type
     * It ensures that no two consecutive characters are the same and that the password meets the required character type distribution
     */
    public static string RunGeneratePassword(int length, PasswordType type) {
        string charPool = GetCharacterPool(type);
        char[] password = new char[length];
        char lastUsedChar = '\0';

        while (true) {
            for (int i = 0; i < length; i++) { // Generate a random character from the pool, ensuring it's not the same as the last used character
                char nextChar;
                do {
                    int randomIndex = RandomNumberGenerator.GetInt32(0, charPool.Length);
                    nextChar = charPool[randomIndex];
                }
                while (nextChar == lastUsedChar);

                password[i] = nextChar;
                lastUsedChar = nextChar;
            }

            if (IsPasswordValid(new string(password), length, type))                    // Validate the generated password against the required character type distribution
                return new string(password);
        }
    }

    /* Method to validate the generated password based on the required character type distribution
     * It checks if the password contains at least a certain number of characters from each required category (numeric, alphabetic, special)
     */
    static bool IsPasswordValid(string password, int length, PasswordType type) {
        int requiredCount = length / 4;

        if (password.Count(NumericChars.Contains) < requiredCount)
            return false;

        if (type == PasswordType.Alphanumeric || type == PasswordType.AlphanumericSpecial)
            if (password.Count(AlphaChars.Contains) < requiredCount)
                return false;

        if (type == PasswordType.AlphanumericSpecial)
            if (password.Count(SpecialChars.Contains) < requiredCount)
                return false;
        return true;
    }
}
