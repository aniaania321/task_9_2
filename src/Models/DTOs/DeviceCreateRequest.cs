using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class DeviceCreateRequest
{
    [Required]
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; }
    public int? DeviceTypeId { get; set; }

    
}