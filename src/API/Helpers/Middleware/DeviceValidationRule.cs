using API.Helpers.Middleware;

public class DeviceValidationRule
{
    public string Type { get; set; }
    public string PreRequestName { get; set; }
    public string PreRequestValue { get; set; }
    public List<ParamRule> Rules { get; set; }
}