namespace API;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using API.Data;
using API.Services.Tokens;
using Models.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly _2019sbdContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<Account> _passwordHasher;

    public AuthorizationController(
        _2019sbdContext context,
        ITokenService tokenService,
        IPasswordHasher<Account> passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == request.Username);
        if (account == null)
            return Unauthorized();

        var verificationResult = _passwordHasher.VerifyHashedPassword(account, account.Password, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            return Unauthorized();

        var role = await _context.Roles
            .Where(r => r.Id == account.RoleId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync() ?? "User";

        var token = _tokenService.CreateToken(account, role);
        return Ok(new { Token = token });
    }
}