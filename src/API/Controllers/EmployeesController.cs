using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using API.Data;
using Models.Models;

namespace API;

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

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetMyAccount()
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var account = await _context.Accounts
            .Include(a => a.Employee)
                .ThenInclude(e => e.Person)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null || account.Employee == null || account.Employee.Person == null)
            return NotFound();

        var person = account.Employee.Person;

        return Ok(new
        {
            account.Id,
            account.Username,
            person.FirstName,
            person.MiddleName,
            person.LastName,
            person.Email,
            person.PhoneNumber
        });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateMyAccount([FromBody] Account update)
    {
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var account = await _context.Accounts
            .Include(a => a.Employee)
                .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null || account.Employee == null || account.Employee.Person == null)
            return NotFound();

        account.Username = update.Username;

        if (!string.IsNullOrWhiteSpace(update.Password))
            account.Password = _passwordHasher.HashPassword(account, update.Password);

        account.Employee.Person.FirstName = update.Employee?.Person?.FirstName ?? account.Employee.Person.FirstName;
        account.Employee.Person.MiddleName = update.Employee?.Person?.MiddleName;
        account.Employee.Person.LastName = update.Employee?.Person?.LastName ?? account.Employee.Person.LastName;
        account.Employee.Person.Email = update.Employee?.Person?.Email ?? account.Employee.Person.Email;
        account.Employee.Person.PhoneNumber = update.Employee?.Person?.PhoneNumber ?? account.Employee.Person.PhoneNumber;

        await _context.SaveChangesAsync();

        return Ok(new { account.Id, account.Username });
    }
}
