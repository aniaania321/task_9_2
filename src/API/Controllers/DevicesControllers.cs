using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Models.DTOs;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _service;
    private readonly _2019sbdContext _context;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(IDeviceService service, _2019sbdContext context, ILogger<DevicesController> logger)
    {
        _service = service;
        _context = context;
        _logger = logger;
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAll()
    {
        try
        {
            var devices = _service.GetAll();
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all devices.");
            return StatusCode(500, new { message = "Failed to retrieve devices.", detail = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var deviceDto = _service.GetById(id);
            if (deviceDto == null)
                return NotFound(new { message = $"Device with id {id} not found." });

            var role = User.FindFirstValue(ClaimTypes.Role);
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdClaim, out int accountId))
                return Forbid();

            if (role == "Admin")
                return Ok(deviceDto);

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
            if (account?.EmployeeId == null)
                return Forbid();

            var isAssigned = await _context.DeviceEmployees
                .AnyAsync(de => de.DeviceId == id && de.EmployeeId == account.EmployeeId);

            return isAssigned ? Ok(deviceDto) : Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve device with id {DeviceId}.", id);
            return StatusCode(500, new { message = "Failed to retrieve device.", detail = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create([FromBody] DeviceCreateRequest device)
    {
        try
        {
            var created = _service.Create(device);
            return Ok(created);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create device.");
            return StatusCode(500, new { message = "Failed to create device.", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] DeviceCreateRequest request)
    {
        try
        {
            var existing = _service.GetById(id);
            if (existing == null)
                return NotFound(new { message = $"Device with id {id} not found." });

            var role = User.FindFirstValue(ClaimTypes.Role);
            var accountIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(accountIdClaim, out int accountId))
                return Forbid();

            if (role != "Admin")
            {
                var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
                if (account?.EmployeeId == null)
                    return Forbid();

                var isAssigned = await _context.DeviceEmployees
                    .AnyAsync(de => de.DeviceId == id && de.EmployeeId == account.EmployeeId);

                if (!isAssigned)
                    return Forbid();
            }

            var updated = _service.Update(id, request);
            return updated == null
                ? NotFound(new { message = $"Failed to update device with id {id}." })
                : Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update device with id {DeviceId}.", id);
            return StatusCode(500, new { message = "Failed to update device.", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        try
        {
            var success = _service.Delete(id);
            return success
                ? NoContent()
                : NotFound(new { message = $"Device with id {id} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete device with id {DeviceId}.", id);
            return StatusCode(500, new { message = "Failed to delete device.", detail = ex.Message });
        }
    }
}
