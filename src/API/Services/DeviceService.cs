using System.Text.Json;
using System.Text.RegularExpressions;
using API.Data;
using Models.DTOs;
using Models.Models;

namespace API;

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
        var fixedJson = FixJson(d.AdditionalProperties ?? "{}");
        var additionalProps = JsonSerializer.Deserialize<Dictionary<string, string>>(fixedJson) ?? new();

        return new DeviceDetailsDto
        {
            Name = d.Name,
            IsEnabled = d.IsEnabled,
            Type = deviceType?.Name,
            AdditionalProperties = additionalProps
        };
    }

    public DeviceDetailsDto Create(DeviceCreateRequest request)
    {
        var d = new Device
        {
            Name = request.Name,
            IsEnabled = request.IsEnabled,
            AdditionalProperties = JsonSerializer.Serialize(request.AdditionalProperties),
            DeviceTypeId = request.TypeId
        };

        _context.Devices.Add(d);
        _context.SaveChanges();

        return new DeviceDetailsDto
        {
            Name = d.Name,
            IsEnabled = d.IsEnabled,
            Type = _context.DeviceTypes.Find(d.DeviceTypeId)?.Name,
            AdditionalProperties = JsonSerializer.Deserialize<Dictionary<string, string>>(FixJson(d.AdditionalProperties)) ?? new()
        };
    }

    
    public DeviceDetailsDto Update(int id, DeviceCreateRequest request)
    {
        var existing = _context.Devices.Find(id);
        if (existing == null) return null;

        existing.Name = request.Name;
        existing.IsEnabled = request.IsEnabled;
        existing.AdditionalProperties = JsonSerializer.Serialize(request.AdditionalProperties);
        existing.DeviceTypeId = request.TypeId;

        _context.Devices.Update(existing);
        _context.SaveChanges();

        return new DeviceDetailsDto
        {
            Name = existing.Name,
            IsEnabled = existing.IsEnabled,
            Type = _context.DeviceTypes.Find(existing.DeviceTypeId)?.Name,
            AdditionalProperties = JsonSerializer.Deserialize<Dictionary<string, string>>(FixJson(existing.AdditionalProperties)) ?? new()
        };
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