using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Models.DTOs;

public class DeviceCreateRequest
{
    [Required]
    public string Name { get; set; }
    [Required]
    public bool IsEnabled { get; set; }
    [Required]
    public JsonElement AdditionalProperties { get; set; }
    [Required]
    public int? TypeId { get; set; }

    
}