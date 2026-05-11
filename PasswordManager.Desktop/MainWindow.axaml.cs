using Avalonia.Controls;
using Avalonia.Interactivity;
using PasswordManager.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManager.Desktop;

public partial class MainWindow : Window {
    private PasswordApiClient? _apiClient;
    private ObservableCollection<Account> _allAccounts = [];

    public MainWindow() {
        InitializeComponent();
        SetSettingsFromDialog();

        _ = LoadAccountsList();
    }

    private void SetSettingsFromDialog() {
        string url = SettingsWindow.GetServerUrl();
        string apiKey = SettingsWindow.GetApiKey();
        if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(apiKey)) {
            NetworkKeyManager.SetManualKey(apiKey);
            _apiClient = new PasswordApiClient(url, apiKey);
        }
    }

    private async Task LoadAccountsList() {
        if (_apiClient == null) {
            LstPasswords.ItemsSource = null;
            return;
        }

        try {
            var accounts = await _apiClient.GetAccountsAsync();
            _allAccounts = new ObservableCollection<Account>(accounts);
            LstPasswords.ItemsSource = _allAccounts;
        }
        catch (Exception ex) {
            // In a real app, show a dialog to the user
            System.Diagnostics.Debug.WriteLine($"Failed to load accounts: {ex.Message}");
        }
    }

    private void TxtSearch_TextChanged(object? sender, TextChangedEventArgs e) {
        string searchText = TxtSearch.Text?.ToLower() ?? "";

        var filteredAccounts = _allAccounts.Where(a =>
            (a.Title?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (a.Username?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();

        LstPasswords.ItemsSource = filteredAccounts;
    }

    private void LstPasswords_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (LstPasswords.SelectedItem is Account selectedEntry) {
            TxtEntryTitle.Text = selectedEntry.Title;
            TxtEntryWebsite.Text = selectedEntry.Site ?? "";
            TxtUsername.Text = selectedEntry.Username;
            TxtPassword.Text = selectedEntry.Password;
        }
    }

    private void AddNewEntryButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        var newAccount = new Account("New Account", "", "", "");

        _allAccounts.Add(newAccount);
        TxtSearch.Text = "";
        LstPasswords.SelectedItem = newAccount;
    }

    private async void BtnCopyUsername_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (LstPasswords.SelectedItem is Account selectedEntry) {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
                await clipboard.SetTextAsync(selectedEntry.Username);
        }
    }

    private void ShowPassword_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        TxtPassword.RevealPassword = !TxtPassword.RevealPassword;
    }

    private async void CopyPassword_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (LstPasswords.SelectedItem is Account selectedEntry) {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
                await clipboard.SetTextAsync(selectedEntry.Password);
        }
    }

    private async void BtnSave_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        /*if (LstPasswords.SelectedItem is Account selectedEntry) {
            if (!string.IsNullOrEmpty(TxtEntryTitle.Text) &&
                !string.IsNullOrEmpty(TxtEntryWebsite.Text) &&
                !string.IsNullOrEmpty(TxtUsername.Text) &&
                !string.IsNullOrEmpty(TxtPassword.Text)) {
                int savedIndex = LstPasswords.SelectedIndex;

                if (selectedEntry.Id > 0)
                    await _apiClient.ChangeAccountAsync(selectedEntry.Id, TxtEntryTitle.Text, TxtEntryWebsite.Text, TxtUsername.Text, TxtPassword.Text);
                else
                    await _apiClient.AddAccountAsync(TxtEntryTitle.Text, TxtEntryWebsite.Text, TxtUsername.Text, TxtPassword.Text);

                await LoadAccountsList();

                if (LstPasswords.ItemsSource is List<Account> currentItems && savedIndex >= 0 && savedIndex < currentItems.Count)
                    LstPasswords.SelectedIndex = savedIndex;
            }
        }*/

        if (_apiClient == null || LstPasswords.SelectedItem is not Account selectedEntry) 
            return;

        if (string.IsNullOrWhiteSpace(TxtEntryTitle.Text) || string.IsNullOrWhiteSpace(TxtUsername.Text) || string.IsNullOrWhiteSpace(TxtPassword.Text)) {
            return; // In a real app, show "Fields cannot be empty"
        }

        try {
            int savedIndex = LstPasswords.SelectedIndex;

            if (selectedEntry.Id > 0) {
                await _apiClient.ChangeAccountAsync(selectedEntry.Id, TxtEntryTitle.Text, TxtEntryWebsite.Text ?? "", TxtUsername.Text, TxtPassword.Text);
            }
            else {
                await _apiClient.AddAccountAsync(TxtEntryTitle.Text, TxtEntryWebsite.Text ?? "", TxtUsername.Text, TxtPassword.Text);
            }

            await LoadAccountsList();

            if (savedIndex >= 0 && LstPasswords.ItemCount > savedIndex)
                LstPasswords.SelectedIndex = savedIndex;
        }
        catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
        }
    }

    private async void BtnDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        /*if (LstPasswords.SelectedItem is Account selectedEntry && selectedEntry.Id > 0) {
            await _apiClient.DeleteAccountAsync(selectedEntry.Id);
            await LoadAccountsList();
        }*/

        if (_apiClient == null || LstPasswords.SelectedItem is not Account selectedEntry) 
            return;

        if (selectedEntry.Id > 0) {
            try {
                await _apiClient.DeleteAccountAsync(selectedEntry.Id);
                await LoadAccountsList();
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}"); // In a real app, show a dialog to the user
            }
        }
        else {
            _allAccounts.Remove(selectedEntry);
        }
    }

    private void PasswordGeneratorButton_Click(object sender, RoutedEventArgs e) {
        var win = new PasswordGenerator.Desktop.MainWindow();
        win.Show();
    }

    private async void BtnSettings_Click(object? sender, RoutedEventArgs e) {
        var dlg = new SettingsWindow();
        await dlg.ShowDialog(this);

        SetSettingsFromDialog();

        await LoadAccountsList();
    }

    private async Task CopyToClipboard(string? text) {
        if (string.IsNullOrEmpty(text)) return;
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null) {
            await clipboard.SetTextAsync(text);
        }
    }
}