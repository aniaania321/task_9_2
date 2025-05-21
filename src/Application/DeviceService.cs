using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Models.DTOs;
using Models.Models;

namespace Application;

public class DeviceService : IDeviceService
{
    private readonly _2019sbdContext _context;
    public DeviceService(_2019sbdContext context)
    {
        _context = context;
    }

    public List<DeviceDto> GetAll() =>
        _context.Devices
            .Select(d => new DeviceDto { Id = d.Id, Name = d.Name })
            .ToList();

    public DeviceDetailsDto GetById(int id)
    {
        var d = _context.Devices.Find(id);
        if (d == null) return null;

        var deviceType = _context.DeviceTypes.Find(d.DeviceTypeId);
        var currentAssignment = _context.DeviceEmployees
            .Include(de => de.Employee)
            .ThenInclude(e => e.Person)
            .FirstOrDefault(de => de.DeviceId == d.Id && de.ReturnDate == null);

        var fixedJson = FixJson(d.AdditionalProperties ?? "{}");
        var additionalProperties = JsonSerializer.Deserialize<Dictionary<string, string>>(fixedJson) ?? new Dictionary<string, string>();

        return new DeviceDetailsDto()
        {
            Id = d.Id,
            Name = d.Name,
            IsEnabled = d.IsEnabled,
            DeviceTypeName = deviceType?.Name,
            AdditionalProperties = additionalProperties,
            CurrentEmployee = currentAssignment != null ? new CurrentEmployeeDto
            {
                Id = currentAssignment.Employee.Id,
                FullName = $"{currentAssignment.Employee.Person.FirstName} {currentAssignment.Employee.Person.LastName}"
            } : null
        };
    }

    public DeviceDetailsDto Create(DeviceCreateRequest request)
    {
        var d = new Device
        {
            Name = request.Name,
            IsEnabled = request.IsEnabled,
            AdditionalProperties = JsonSerializer.Serialize(request.AdditionalProperties),
            DeviceTypeId = request.DeviceTypeId
        };

        _context.Devices.Add(d);
        _context.SaveChanges();

        return new DeviceDetailsDto { Id = d.Id, Name = d.Name };
    }
    
    public DeviceDetailsDto Update(int id, DeviceCreateRequest request)
    {
        var existing = _context.Devices.Find(id);
        if (existing == null) return null;

        existing.Name = request.Name;
        existing.IsEnabled = request.IsEnabled;
        existing.AdditionalProperties = JsonSerializer.Serialize(request.AdditionalProperties);
        existing.DeviceTypeId = request.DeviceTypeId;

        _context.Devices.Update(existing);
        _context.SaveChanges();

        return new DeviceDetailsDto { Id = existing.Id, Name = existing.Name };
    }
    
    public bool Delete(int id)
    {
        var device = _context.Devices.Find(id);
        if (device == null) return false;

        _context.Devices.Remove(device);
        _context.SaveChanges();
        return true;
    }

    string FixJson(string json)//I added this because i was getting a formatting issue where the special properties was null because it didn't have "", I didn't want to change the database so I manually add the ""
    {
        return Regex.Replace(json, @"(?<=\{|,)\s*(\w+)\s*:", "\"$1\":");
    }

}