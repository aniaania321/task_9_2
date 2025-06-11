using API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.DTOs;
using Models.Models;

namespace API.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly _2019sbdContext _context;
    private readonly IPasswordHasher<Account> _passwordHasher;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(_2019sbdContext context, IPasswordHasher<Account> passwordHasher, ILogger<AccountsController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (await _context.Accounts.AnyAsync(a => a.Username == request.Username))
                return Conflict(new { message = "Username must be unique!." });

            var account = new Account
            {
                Username = request.Username,
                EmployeeId = request.EmployeeId,
                RoleId = request.RoleId,
                Password = _passwordHasher.HashPassword(null, request.Password)
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, new AccountDto
            {
                Id = account.Id,
                Username = account.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account.");
            return StatusCode(500, new { message = "Server error", detail = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        try
        {
            var acc = await _context.Accounts
                .Include(a => a.Role)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (acc == null)
                return NotFound(new { message = $"Account with id {id} was not found." });

            return Ok(new AccountDetailsDto
            {
                Username = acc.Username,
                Role = acc.Role?.Name ?? "Unknown"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account by ID.");
            return StatusCode(500, new { message = "Error getting account", detail = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        try
        {
            var accounts = await _context.Accounts
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    Username = a.Username
                }).ToListAsync();

            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get accounts.");
            return StatusCode(500, new { message = "Error getting accounts", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] RegisterRequest request)
    {
        try
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null)
                return NotFound(new { message = $"Account with id {id} not found." });

            if (acc.Username != request.Username &&
                await _context.Accounts.AnyAsync(a => a.Username == request.Username && a.Id != id))
            {
                return Conflict(new { message = "Username already exists." });
            }

            acc.Username = request.Username;
            acc.Password = _passwordHasher.HashPassword(acc, request.Password);
            acc.EmployeeId = request.EmployeeId;
            acc.RoleId = request.RoleId;

            await _context.SaveChangesAsync();

            return Ok(new AccountDto { Id = acc.Id, Username = acc.Username });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating account.");
            return StatusCode(500, new { message = "Error updating account", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        try
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null)
                return NotFound(new { message = $"Account with id {id} not found." });

            _context.Accounts.Remove(acc);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting account.");
            return StatusCode(500, new { message = "Error deleting account", detail = ex.Message });
        }
    }
}
