using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Models.DTOs;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _service;
    private readonly _2019sbdContext _context;

    public DevicesController(IDeviceService service, _2019sbdContext context)
    {
        _service = service;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAll() => Ok(_service.GetAll());

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        var deviceDto = _service.GetById(id);
        if (deviceDto == null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role);
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (role == "Admin")
            return Ok(deviceDto);

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null || account.EmployeeId == null)
            return Forbid();

        var isAssigned = await _context.DeviceEmployees
            .AnyAsync(de => de.DeviceId == id && de.EmployeeId == account.EmployeeId);

        return isAssigned ? Ok(deviceDto) : Forbid();
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create(DeviceCreateRequest device)
    {
        var created = _service.Create(device);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, DeviceCreateRequest request)
    {
        var existing = _service.GetById(id);
        if (existing == null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role);
        var accountId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (role != "Admin")
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId);

            if (account?.EmployeeId == null)
                return Forbid();

            var isAssigned = await _context.DeviceEmployees
                .AnyAsync(de => de.DeviceId == id && de.EmployeeId == account.EmployeeId);

            if (!isAssigned)
                return Forbid();
        }

        var updated = _service.Update(id, request);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        return _service.Delete(id) ? NoContent() : NotFound();
    }
}
