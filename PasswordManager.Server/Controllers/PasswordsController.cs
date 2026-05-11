using Microsoft.AspNetCore.Mvc;
using PasswordManager.Core;

namespace PasswordManager.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordsController : ControllerBase {
        private readonly PasswordRepository _manager;


        public PasswordsController(PasswordRepository manager) {
            _manager = manager;
        }

        // GET: api/passwords
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts() {
            var accounts = await _manager.GetAccountsFromDBAsync();
            var safeAccounts = accounts.Select(account => new {
                account.Id,
                account.Title,
                account.Site,
                account.Username,
                EncryptedPassword = NetworkTransit.EncryptPayload(account.Password)
            }).ToList();
            return Ok(safeAccounts);
        }

        // GET: api/passwords/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(int id) {
            var account = await _manager.GetAccountFromDBAsync(id);
            if (account == null) return NotFound("Account not found.");
            var safeResponse = new {
                account.Id,
                account.Title,
                account.Site,
                account.Username,
                EncryptedPassword = NetworkTransit.EncryptPayload(account.Password)
            };
            return Ok(safeResponse);
        }

        // POST: api/passwords
        [HttpPost]
        public async Task<IActionResult> AddAccount([FromBody] AccountRequest request) {
            if (string.IsNullOrEmpty(request.EncryptedPassword))
                return BadRequest("Password cannot be empty.");

            string plainTextPassword = NetworkTransit.DecryptPayload(request.EncryptedPassword);

            await _manager.AddAccountToDBAsync(
                request.Title,
                request.Site,
                request.Username,
                plainTextPassword
            );
            return Ok(new { Message = "Account added successfully." });
        }

        // PUT: api/passwords/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountRequest request) {
            if (string.IsNullOrEmpty(request.EncryptedPassword))
                return BadRequest("Password cannot be empty.");

            string plainTextPassword = NetworkTransit.DecryptPayload(request.EncryptedPassword);

            await _manager.ChangeAccountInDBAsync(
                id,
                request.Title,
                request.Site,
                request.Username,
                plainTextPassword
            );
            return Ok(new { Message = "Account updated." });
        }

        // DELETE: api/passwords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id) {
            await _manager.DeleteAccountFromDBAsync(id);
            return Ok(new { Message = "Account deleted." });
        }
    }

    public class AccountRequest {
        public string Title { get; set; } = "";
        public string Site { get; set; } = "";
        public string Username { get; set; } = "";
        public string EncryptedPassword { get; set; } = "";
    }
}