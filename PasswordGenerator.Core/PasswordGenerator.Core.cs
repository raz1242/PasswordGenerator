using System;
using System.Linq;
using System.Security.Cryptography;

namespace PasswordGenerator.Core
{
    public enum PasswordType {
        AlphanumericSpecial = 1,
        Alphanumeric = 2,
        Numeric = 3
    }

    public static class GeneratePassword {
        const string NumericChars = "0123456789";
        const string AlphaChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string SpecialChars = "!@#$%^&*()_+";


        static string GetCharacterPool(PasswordType type) {
            return type switch {
                PasswordType.AlphanumericSpecial => NumericChars + AlphaChars + SpecialChars,
                PasswordType.Alphanumeric => NumericChars + AlphaChars,
                PasswordType.Numeric => NumericChars,
                _ => NumericChars + AlphaChars + SpecialChars
            };
        }

        public static string RunGeneratePassword(int length, PasswordType type) {
            string charPool = GetCharacterPool(type);
            char[] password = new char[length];
            char lastUsedChar = '\0';

            while (true) {
                for (int i = 0; i < length; i++) {
                    char nextChar;
                    do {
                        int randomIndex = RandomNumberGenerator.GetInt32(0, charPool.Length);
                        nextChar = charPool[randomIndex];
                    }
                    while (nextChar == lastUsedChar);

                    password[i] = nextChar;
                    lastUsedChar = nextChar;
                }

                if (IsPasswordValid(new string(password), length, type))
                    return new string(password);
            }
        }

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

        /*
        static int GetPasswordLength() {
            while (true) {
                Console.Write("Enter password length (8-24 characters, default's 16): ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) // Check if the input is empty or whitespace
                    return 16;
                else if (!input.All(char.IsDigit)) { // Check if the input contains only digits
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }
                else if (int.Parse(input) < 8 || int.Parse(input) > 24) { // Check if the number is within the valid range
                    Console.WriteLine("Invalid input. Please enter a number between 8 and 20.");
                    continue;
                }
                return int.Parse(input);
            }
        }
        static PasswordType GetPasswordType() {
            while (true) {
                Console.Write("Choose password type:\n1. Alphanumeric with special characters\n2. Alphanumeric \n3. Numeric only\n(Default's 1): ");
                string type = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(type) || type == "1")
                    return PasswordType.AlphanumericSpecial;
                else if (type == "2")
                    return PasswordType.Alphanumeric;
                else if (type == "3")
                    return PasswordType.Numeric;
                else
                    Console.WriteLine("Invalid input. Please enter 1, 2, or 3.");
            }
        
        }*/
    }
}
