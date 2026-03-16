using Avalonia.Controls;
using PasswordManager.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PasswordManager.Desktop;
public partial class MainWindow : Window {
    private readonly Core.PasswordManager _manager;


    public MainWindow() {
        InitializeComponent();

        string myDbPath = Path.Combine(AppContext.BaseDirectory, "PasswordsData.db");
        _manager = new Core.PasswordManager(myDbPath);

        _ = LoadAccountsList();
    }

    private async Task LoadAccountsList(List<int>? indexes = null) {
        LstPasswords.Items.Clear();

        if (indexes != null) {
            foreach (var index in indexes) {
                Account? account = await _manager.GetAccountFromDBAsync(index);
                if (account != null)
                    LstPasswords.Items.Add(account);
            }
        }
        else {
            List<Account> accounts = await _manager.GetAccountsFromDBAsync();
            foreach (var account in accounts) {
                if (account != null)
                    LstPasswords.Items.Add(account);
            }
        }
    }

    private async void TxtSearch_TextChanged(object? sender, TextChangedEventArgs e) {
        string searchText = TxtSearch.Text ?? "";
        List<int> indexes = await _manager.SearchAccountsInDBAsync(searchText);
        await LoadAccountsList(indexes);
    }

    private void LstPasswords_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (LstPasswords.SelectedItem is Account selectedEntry) {
            TxtEntryTitle.Text = selectedEntry.Title;
            if (selectedEntry.Site != null)
                TxtEntryWebsite.Text = selectedEntry.Site;
            else
                TxtEntryWebsite.Text = "";
            TxtUsername.Text = selectedEntry.Username;
            TxtPassword.Text = selectedEntry.Password;
            TxtNotes.Text = selectedEntry.Notes;
        }
    }

    private void AddNewEntryButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        Account new_account = new("", "", "");
        LstPasswords.Items.Add(new_account);
        LstPasswords.SelectedItem = new_account;
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
                await clipboard.SetTextAsync(selectedEntry.Username);
        }
    }

    private async void BtnSave_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (LstPasswords.SelectedItem is Account selectedEntry)
            if (!string.IsNullOrEmpty(TxtEntryTitle.Text) &&
                !string.IsNullOrEmpty(TxtEntryWebsite.Text) &&
                !string.IsNullOrEmpty(TxtUsername.Text) &&
                !string.IsNullOrEmpty(TxtPassword.Text)) {
                if (selectedEntry.Id >= 0)
                    await _manager.ChangeAccountInDBAsync(selectedEntry.Id, TxtEntryTitle.Text, TxtEntryWebsite.Text, TxtUsername.Text, TxtPassword.Text);
                else
                    await _manager.AddAccountToDBAsync(TxtEntryTitle.Text, TxtEntryWebsite.Text, TxtUsername.Text, TxtPassword.Text);

                await LoadAccountsList();
            }
    }

    private async void BtnDelete_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) {
        if (LstPasswords.SelectedItem is Account selectedEntry) {
            await _manager.DeleteAccountFromDBAsync(selectedEntry.Id);
            await LoadAccountsList();
        }
    }

    private void PasswordGeneratorButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) {
        PasswordGenerator.Desktop.MainWindow passwordGeneratorWindow = new();
        passwordGeneratorWindow.Show();
    }
}