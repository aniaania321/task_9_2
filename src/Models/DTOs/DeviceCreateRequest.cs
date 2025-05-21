using System.ComponentModel.DataAnnotations;

namespace Models.DTOs;

public class DeviceCreateRequest
{
    [Required]
    public string Name { get; set; }
    [Required]
    public bool IsEnabled { get; set; }
    [Required]
    public Dictionary<string, string> AdditionalProperties { get; set; }
    [Required]
    public int? DeviceTypeId { get; set; }

    
}