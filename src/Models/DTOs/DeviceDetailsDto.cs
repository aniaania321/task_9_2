namespace Models.DTOs;

public class DeviceDetailsDto
{
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; }
    public string Type { get; set; }
}