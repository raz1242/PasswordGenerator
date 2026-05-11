using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using PasswordManager.Core;

namespace PasswordManager.Desktop {
    public partial class SettingsWindow : Window {

        // Simple flat settings file next to the exe
        //private const string SettingsFile = "settings.cfg";
        private static readonly string SettingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PasswordManager",
        "settings.cfg"
        );

        public SettingsWindow() {
            InitializeComponent();
            LoadSettings();
        }
        
        // ── Load / Save ────────────────────────────────────────────────
        private void LoadSettings() {
            if (!File.Exists(SettingsFile)) 
                return;

            var lines = File.ReadAllLines(SettingsFile);
            foreach (var line in lines) {
                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                switch (parts[0].Trim()) {
                    case "ServerUrl": TxtServerUrl.Text = parts[1].Trim(); break;
                    case "ApiKey": TxtApiKey.Text = parts[1].Trim(); break;
                }
            }

            /*if (!string.IsNullOrEmpty(TxtApiKey.Text)) {
                NetworkKeyManager.SetManualKey(TxtApiKey.Text);
            }*/
        }

        private void SaveSettings() {
            string url = TxtServerUrl.Text?.Trim() ?? "";
            string key = TxtApiKey.Text?.Trim() ?? "";

            var lines = new[] {
                $"ServerUrl={url}",
                $"ApiKey={key}"
            };

            string? directory = Path.GetDirectoryName(SettingsFile);
            if (!string.IsNullOrEmpty(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(SettingsFile, lines);

            if (!string.IsNullOrEmpty(key)) {
                NetworkKeyManager.SetManualKey(key);
            }


        }

        // Public helpers so MainWindow can read the saved values
        public static string GetServerUrl() => ReadKey("ServerUrl") ?? "http://127.0.0.1:5000";
        public static string GetApiKey() => ReadKey("ApiKey") ?? "";

        private static string? ReadKey(string key) {
            if (!File.Exists(SettingsFile)) return null;
            foreach (var line in File.ReadAllLines(SettingsFile)) {
                var p = line.Split('=', 2);
                if (p.Length == 2 && p[0].Trim() == key) return p[1].Trim();
            }
            return null;
        }

        // ── Button handlers ────────────────────────────────────────────
        private void BtnRevealKey_Click(object? sender, RoutedEventArgs e) {
            bool hidden = TxtApiKey.PasswordChar == '*';
            TxtApiKey.PasswordChar = hidden ? '\0' : '*';
            BtnRevealKey.Content = hidden ? "Hide" : "Show";
        }

        private void BtnSaveSettings_Click(object? sender, RoutedEventArgs e) {
            string url = TxtServerUrl.Text?.Trim() ?? "";
            string key = TxtApiKey.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(TxtServerUrl.Text) || string.IsNullOrWhiteSpace(TxtApiKey.Text)) {
                ShowStatus("Error: Both fields are required.");
                return;
            }

            bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                      && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!isValidUrl) {
                ShowStatus("Error: Invalid URL. (Ex: http://127.0.0.1:5000)");
                return;
            }

            if (!NetworkKeyManager.SetManualKey(key)) {
                ShowStatus("Error: Invalid Key Format (Base64 expected)");
                return;
            }

            SaveSettings();
            ShowStatus("Settings saved successfully!");
            this.Close();
        }

        private void BtnCancelSettings_Click(object? sender, RoutedEventArgs e) {
            Close();
        }

        private void ShowStatus(string msg) {
            TxtStatus.Text = msg;
        }
    }
}