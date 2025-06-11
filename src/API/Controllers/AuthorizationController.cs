using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using API.Data;
using API.Services.Tokens;
using Models.DTOs;
using Models.Models;
using Microsoft.Extensions.Logging;

namespace API;

[ApiController]
[Route("api/authorization")]
public class AuthorizationController : ControllerBase
{
    private readonly _2019sbdContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<Account> _passwordHasher;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(
        _2019sbdContext context,
        ITokenService tokenService,
        IPasswordHasher<Account> passwordHasher,
        ILogger<AuthorizationController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogError("Username or password missing.");
                return BadRequest(new { message = "Username and password are required." });
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == request.Username);
            if (account == null)
            {
                _logger.LogError("Account not found for username '{Username}'.", request.Username);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var result = _passwordHasher.VerifyHashedPassword(account, account.Password, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogError("Invalid password for username '{Username}'.", request.Username);
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var role = await _context.Roles
                .Where(r => r.Id == account.RoleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? "User";

            var token = _tokenService.CreateToken(account, role);

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication.");
            return StatusCode(500, new { message = "Error during authentication.", detail = ex.Message });
        }
    }
}
