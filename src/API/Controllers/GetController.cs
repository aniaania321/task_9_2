using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;

namespace API;

[ApiController]
[Route("api")]
public class GetController : ControllerBase //controller for all the ones i needed to add in this task
{
    private readonly _2019sbdContext _context;  

    public GetController(_2019sbdContext context)
    {
        _context = context;
    }

    [HttpGet("devices/types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDeviceTypes()
    {
        var types = await _context.DeviceTypes
            .Select(dt => new { dt.Id, dt.Name })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("roles")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("positions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPositions()
    {
        var positions = await _context.Positions
            .Select(p => new { p.Id, p.Name })
            .ToListAsync();

        return Ok(positions);
    }
}