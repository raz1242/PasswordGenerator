using PasswordManager.Core;

namespace PasswordManager.Server {
    public class ApiKeyMiddleware {
        private readonly RequestDelegate _next;
        private const string APIKEYNAME = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) {
            // 1. Always let Swagger through
            if (context.Request.Path.StartsWithSegments("/swagger")) {
                await _next(context);
                return;
            }

            // 2. Reject if header is missing
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey)) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key missing.");
                return;
            }

            // 3. Get the "Source of Truth" key from the NetworkKeyManager
            // We convert the byte[] to Base64 so we can compare it to the string header
            byte[] keyBytes = NetworkKeyManager.GetKey();
            string validApiKey = Convert.ToBase64String(keyBytes);

            // 4. Compare them
            if (extractedApiKey != validApiKey) {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid API Key.");
                return;
            }

            await _next(context);
        }
    }
}