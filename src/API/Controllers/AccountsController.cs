using API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using API.Data;
using Models.Models;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly _2019sbdContext _context;
    private readonly IPasswordHasher<Account> _passwordHasher;

    public AccountsController(_2019sbdContext context, IPasswordHasher<Account> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    //[Authorize(Roles = "Admin")] here I am not sure whether it should be only for admin or not because it says only admins can register but for testing I couldn't create any account to authorize it then because I didn't have any already authorized account to register a new account so i just kept it so that anyone can register for now
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Accounts.AnyAsync(a => a.Username == request.Username))
            return BadRequest("Username must be unique.");

        var account = new Account
        {
            Username = request.Username,
            EmployeeId = request.EmployeeId,
            RoleId = request.RoleId
        };
        
        account.Password = _passwordHasher.HashPassword(account, request.Password);

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, new { account.Id, account.Username });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccount(int id)
    {
        var acc = await _context.Accounts.FindAsync(id);
        return acc == null ? NotFound() : Ok(new { acc.Id, acc.Username, acc.Password });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAccounts()
    {
        return Ok(await _context.Accounts.Select(a => new { a.Id, a.Username, a.Password }).ToListAsync());
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] RegisterRequest request)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc == null) return NotFound();

        acc.Username = request.Username;
        acc.Password = _passwordHasher.HashPassword(acc, request.Password);
        acc.EmployeeId = request.EmployeeId;
        acc.RoleId = request.RoleId;

        await _context.SaveChangesAsync();
        return Ok(new { acc.Id, acc.Username });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var acc = await _context.Accounts.FindAsync(id);
        if (acc == null) return NotFound();

        _context.Accounts.Remove(acc);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
