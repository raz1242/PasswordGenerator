using PasswordManager.Server;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string folderPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "PasswordManager"
);

if (!Directory.Exists(folderPath)) {
    Directory.CreateDirectory(folderPath);
}

string dbPath = Path.Combine(folderPath, "PasswordsData.db");

EncryptionService encryptionService = new();
builder.Services.AddSingleton<PasswordRepository>(
    new PasswordRepository(dbPath, encryptionService));

var app = builder.Build();

// Enable Swagger even in production if you want to test on the other PC easily
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<PasswordManager.Server.ApiKeyMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();