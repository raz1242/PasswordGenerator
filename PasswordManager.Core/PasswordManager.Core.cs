using Microsoft.Data.Sqlite;

namespace PasswordManager.Core;
public class PasswordRepository {
    private readonly string _accountsConnectionString;
    private readonly IEncryptionService _encryptionService;

    public PasswordRepository(string dbPath, IEncryptionService encryptionService) {
        _accountsConnectionString = $"Data Source={dbPath}";
        _encryptionService = encryptionService;
        InitializeDatabase();
    }

    public void InitializeDatabase() {
        using var connection = new SqliteConnection(_accountsConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Passwords (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                site_name TEXT,
                site_address TEXT,
                username TEXT,
                password BLOB NOT NULL,
                created_at DEFAULT CURRENT_TIMESTAMP
            );";

        command.ExecuteNonQuery();
    }

    public async Task<Account?> GetAccountFromDBAsync(int id) {

        using var connection = new SqliteConnection(_accountsConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT Site_Name, Site_Address, Username, Password FROM Passwords WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        if (await reader.ReadAsync().ConfigureAwait(false)) {
            byte[] encryptedPasswordBytes = (byte[])reader["Password"];
            string decryptedPassword = _encryptionService.Decrypt(encryptedPasswordBytes);

            return new Account(
                reader.IsDBNull(0) ? "" : reader.GetString(0),      // Site_Name
                reader.IsDBNull(1) ? "" : reader.GetString(1),      // Site_Address
                reader.IsDBNull(2) ? "" : reader.GetString(2),      // Username
                decryptedPassword                                   // Password
            );
        }
        return null;
    }


    public async Task<List<Account>> GetAccountsFromDBAsync() {
        var accounts = new List<Account>();

        using var connection = new SqliteConnection(_accountsConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT Site_Name, Site_Address, Username, Password, Id 
                                FROM Passwords 
                                WHERE User_Id = $user_id";
        command.Parameters.AddWithValue("$user_id", 1);

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false)) {
            byte[] encryptedPasswordBytes = (byte[])reader["Password"];
            string decryptedPassword = _encryptionService.Decrypt(encryptedPasswordBytes);

            var account = new Account(
                reader.IsDBNull(0) ? "" : reader.GetString(0),      // Site_Name
                reader.IsDBNull(1) ? "" : reader.GetString(1),      // Site_Address
                reader.IsDBNull(2) ? "" : reader.GetString(2),      // Username
                decryptedPassword                                   // Password
            ) {
                Id = reader.GetInt32(4)                             // Id
            };
            accounts.Add(account);
        }
        return accounts;
    }

    public async Task AddAccountToDBAsync(string siteName, string siteAddress, string username, string password) {
        byte[] encryptedPassword = _encryptionService.Encrypt(password);

        using var connection = new SqliteConnection(_accountsConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();

        command.CommandText = @"INSERT INTO Passwords (User_Id, Site_Name, Site_Address, Username, Password) VALUES 
                                                              ($user_id, $site_name, $site_address, $username, $password)";
        command.Parameters.AddWithValue("$user_id", 1);
        command.Parameters.AddWithValue("$site_name", siteName);
        command.Parameters.AddWithValue("$site_address", siteAddress);
        command.Parameters.AddWithValue("$username", username);
        command.Parameters.Add("$password", SqliteType.Blob).Value = encryptedPassword;

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task ChangeAccountInDBAsync(int id, string siteName, string siteAddress, string username, string password) {
        byte[] encryptedPassword = _encryptionService.Encrypt(password);

        using var connection = new SqliteConnection(_accountsConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        command.CommandText = @"UPDATE Passwords SET Site_Name = $site_name, Site_Address = $site_address, Username = $username, Password = $password WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$site_name", siteName);
        command.Parameters.AddWithValue("$site_address", siteAddress);
        command.Parameters.AddWithValue("$username", username);
        command.Parameters.Add("$password", SqliteType.Blob).Value = encryptedPassword;

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task DeleteAccountFromDBAsync(int id) {
        using var connection = new SqliteConnection(_accountsConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);
        using var command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM Passwords WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<List<int>> SearchAccountsInDBAsync(string searchTerm) {
        List<int> accountsId = [];
        using var connection = new SqliteConnection(_accountsConnectionString);
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT Id From Passwords WHERE Site_Name LIKE $searchTerm";
        command.Parameters.AddWithValue("$searchTerm", $"%{searchTerm}%");

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        while (await reader.ReadAsync().ConfigureAwait(false))
            accountsId.Add(reader.GetInt32(0));

        return accountsId;
    }
}