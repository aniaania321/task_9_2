using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using API.Data;
using Models.DTOs;
using Models.Models;

namespace API;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly _2019sbdContext _context;
    private readonly IPasswordHasher<Account> _passwordHasher;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(
        _2019sbdContext context,
        IPasswordHasher<Account> passwordHasher,
        ILogger<EmployeesController> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    } 

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetMyAccount()
    {
        _logger.LogInformation("GET /api/employees/profile started");

        try
        {
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdClaim, out var accountId))
            {
                _logger.LogError("Failed to parse account ID from claim.");
                return Forbid();
            }

            var account = await _context.Accounts
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account?.Employee?.Person == null)
            {
                _logger.LogError("Employee or Person not found for account ID {AccountId}", accountId);
                return NotFound(new { message = "Associated employee or person record not found." });
            }

            _logger.LogInformation("Profile data retrieved for account ID {AccountId}", accountId);

            var person = account.Employee.Person;
            return Ok(new PersonDto
            {
                PassportNumber = person.PassportNumber,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                Email = person.Email,
                PhoneNumber = person.PhoneNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving profile.");
            return StatusCode(500, new { message = "Failed to retrieve profile.", detail = ex.Message });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateMyAccount([FromBody] PersonDto update)
    {
        _logger.LogInformation("PUT /api/employees/profile started");

        try
        {
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdClaim, out var accountId))
            {
                _logger.LogError("Failed to parse account ID from claim.");
                return Forbid();
            }

            var account = await _context.Accounts
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Person)
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account?.Employee?.Person == null)
            {
                _logger.LogError("Employee or Person not found for account ID {AccountId}", accountId);
                return NotFound(new { message = "Associated employee or person record not found." });
            }

            var person = account.Employee.Person;
            person.FirstName = update.FirstName ?? person.FirstName;
            person.MiddleName = update.MiddleName;
            person.LastName = update.LastName ?? person.LastName;
            person.Email = update.Email ?? person.Email;
            person.PhoneNumber = update.PhoneNumber ?? person.PhoneNumber;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated profile for account ID {AccountId}", accountId);
            return Ok(new { message = "Profile updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while updating profile.");
            return StatusCode(500, new { message = "Failed to update profile.", detail = ex.Message });
        }
    }
}
