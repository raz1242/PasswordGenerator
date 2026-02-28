using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Linq;
using PasswordGenerator.Core;

namespace PasswordGenerator.Desktop {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e) {
            int length = (int)(NumericLength.Value ?? 16);
            PasswordType type;

            if (RadioAlphaSymbols.IsChecked == true)
                type = PasswordType.AlphanumericSpecial;
            else if (RadioAlpha.IsChecked == true)
                type = PasswordType.Alphanumeric;
            else
                type = PasswordType.Numeric;

            String characterSet = GeneratePassword.RunGeneratePassword(length, type);
            TxtResult.Text = characterSet;
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(TxtResult.Text)) {
                CopyToClipboard(TxtResult.Text);
            }
        }

        private async void CopyToClipboard(string text) {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null) {
                await clipboard.SetTextAsync(text);
            }
        }
    }
}