# Password Manager

A self-hosted, client-server password management system built with C#. It features a clean desktop interface and a secure ASP.NET Core backend that utilizes Windows DPAPI for encryption at rest.

## Architecture

The project is split into three main components (N-Tier architecture):
* **PasswordManager.Desktop**: A cross-platform desktop client built with [Avalonia UI](https://avaloniaui.net/).
* **PasswordManager.Server**: An ASP.NET Core Web API that handles data storage and API key validation.
* **PasswordManager.Core**: A shared library containing data models and transit encryption logic.

## Key Features

* **Secure Data Storage**: Passwords are encrypted at rest using Windows DPAPI (`ProtectedData`), ensuring they can only be decrypted by the server's Windows user account.
* **Encrypted Transit**: Custom payload encryption ensures credentials are secure while traveling between the desktop client and the server.
* **API Key Pairing**: The server requires a Pairing Code (`X-API-KEY`) to accept connections, preventing unauthorized access to the database.
* **Desktop UI**: 
  * Add, edit, and delete accounts.
  * Real-time search/filtering.
  * Quick copy buttons for usernames and passwords.
  * Integrated password generator.
* **Local Database**: Lightweight and portable SQLite database (`PasswordsData.db`).

## Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or your target .NET version)
* **Windows OS** (Required for the Server component due to DPAPI encryption. The desktop client can run cross-platform if connected to a Windows server).

## Getting Started

### 1. Run the Server
1. Navigate to the `PasswordManager.Server` directory.
2. Run the application:
   ```bash
   dotnet run
On first run, the server will generate a database inside %APPDATA%\PasswordManager\.
Locate your Pairing Code (API Key). Depending on your setup, this is generated and saved by the server (e.g., in a PAIRING_CODE.txt file).
The API runs on http://localhost:5000 by default. Swagger UI is available at http://localhost:5000/swagger.
2. Run the Desktop Client
Navigate to the PasswordManager.Desktop directory.
Run the application:
code
Bash
dotnet run
Click the Settings (⚙) icon in the top right corner.
Enter the Server URL (e.g., http://localhost:5000) and paste the Pairing Code.
Click Save Settings. The client will now connect to the server and fetch your accounts.
Security Notes
Encryption at Rest: Because the server relies on Windows DPAPI (DataProtectionScope.CurrentUser), the SQLite database file cannot be read if it is copied to another machine or accessed by a different Windows user.
Local Settings: The client stores its connection configuration (including the Pairing Code) in %APPDATA%\PasswordManager\settings.cfg. Keep this file secure.
License
This project is licensed under the MIT License. See the LICENSE file for details.