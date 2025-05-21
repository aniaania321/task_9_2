namespace Models.DTOs;

public class DeviceDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public string DeviceTypeName { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; }
    public CurrentEmployeeDto CurrentEmployee { get; set; }
}