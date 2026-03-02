using Avalonia.Controls;
using Avalonia.Interactivity;
using PasswordGenerator.Core;
using System;

namespace PasswordGenerator.Desktop {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }


        /* Helper method to copy text to the clipboard using Avalonia's clipboard API.
         * It retrieves the clipboard instance from the top-level window and sets the provided text asynchronously.
         */
        private async void CopyToClipboard(string text) {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard; // Get the clipboard instance from the top-level window
            if (clipboard != null) {
                await clipboard.SetTextAsync(text);
            }
        }

        /* Event handler for the "Generate" button click.
         * It retrieves the desired password length and type from the UI,
         * generates the password using the GeneratePassword class, and displays it in the result textbox.
         */
        private void BtnGenerate_Click(object sender, RoutedEventArgs e) {
            int length = (int)(NumericLength.Value ?? 16); // Default to 16 if no value is set
            PasswordType type;

            if (RadioAlphaSymbols.IsChecked == true) // Check if the "Alphanumeric + Symbols" radio button is selected
                type = PasswordType.AlphanumericSpecial;
            else if (RadioAlpha.IsChecked == true) // Check if the "Alphanumeric" radio button is selected
                type = PasswordType.Alphanumeric;
            else // Default to "Numeric"
                type = PasswordType.Numeric;

            String characterSet = GeneratePassword.RunGeneratePassword(length, type); // Generate the password using the core logic
            TxtResult.Text = characterSet; // Display the generated password in the result textbox

            if (!string.IsNullOrEmpty(characterSet)) // If a password was generated, copy it to the clipboard
                CopyToClipboard(characterSet);
        }

        /* Event handler for the "Copy" button click.
         * It checks if there is a generated password in the result textbox and,
         * if so, copies it to the clipboard.
         */
        private void BtnCopy_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(TxtResult.Text)) { // Check if there is a generated password to copy
                CopyToClipboard(TxtResult.Text);
            }
        }
    }
}