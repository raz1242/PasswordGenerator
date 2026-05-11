using PasswordManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PasswordManager.Desktop;

public class PasswordApiClient {
    private readonly HttpClient _http;

    public PasswordApiClient(string serverUrl, string apiKey) {
        _http = new HttpClient { BaseAddress = new Uri(serverUrl) };
        // Clear any existing headers and add your key
        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
    }

    public async Task<List<Account>> GetAccountsAsync() {
        List<AccountDto>? dtos = null;
        try {
            dtos = await _http.GetFromJsonAsync<List<AccountDto>>("api/Passwords");
        }
        catch (Exception ex) {
            // Handle exceptions (e.g., log them, show a message to the user, etc.)
            Console.WriteLine($"Error fetching accounts: {ex.Message}");
            return new List<Account>();
        }
        if (dtos == null || dtos.Count == 0) {
            return new List<Account>();
        }

        // Task.Run pushes this heavy work OFF the UI thread so the app doesn't freeze.
        // AsParallel() tells the CPU to decrypt multiple passwords simultaneously.
        var accounts = await Task.Run(() => {
            return dtos.AsParallel().Select(dto => {
                string plainPassword = NetworkTransit.DecryptPayload(dto.EncryptedPassword);

                return new Account(dto.Title, dto.Site, dto.Username, plainPassword) {
                    Id = dto.Id
                };
            }).ToList();
        });

        return accounts;
    }

    public async Task AddAccountAsync(string title, string website, string username, string plainPassword) {
        string encryptedPassword = NetworkTransit.EncryptPayload(plainPassword);

        var request = new AccountDto {
            Title = title,
            Site = website,
            Username = username,
            EncryptedPassword = encryptedPassword
        };

        await _http.PostAsJsonAsync("api/passwords", request);
    }

    public async Task ChangeAccountAsync(int id, string title, string website, string username, string plainPassword) {
        string encryptedPassword = NetworkTransit.EncryptPayload(plainPassword);
        var request = new AccountDto { Id = id, Title = title, Site = website, Username = username, EncryptedPassword = encryptedPassword };

        await _http.PutAsJsonAsync($"api/passwords/{id}", request);
    }

    public async Task DeleteAccountAsync(int id) {
        await _http.DeleteAsync($"api/passwords/{id}");
    }

    private class AccountDto {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Site { get; set; } = "";
        public string Username { get; set; } = "";
        public string EncryptedPassword { get; set; } = "";
    }
}