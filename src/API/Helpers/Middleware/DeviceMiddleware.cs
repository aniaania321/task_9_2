using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;
using Models.DTOs;

namespace API.Helpers.Middleware;

public class DeviceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DeviceMiddleware> _logger;
    private static List<DeviceValidationRule> _validationRules;

    public DeviceMiddleware(RequestDelegate next, ILogger<DeviceMiddleware> logger)
    {
        _next = next;
        _logger = logger;

        if (_validationRules == null)
        {
            try
            {
                var json = File.ReadAllText("validationRules.json");
                using var doc = JsonDocument.Parse(json);
                _validationRules = JsonSerializer.Deserialize<List<DeviceValidationRule>>(
                    doc.RootElement.GetProperty("validations").GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<DeviceValidationRule>();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to load validation rules:(.");
                _validationRules = new List<DeviceValidationRule>();
            }
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Starting using Middleware");

        var contentType = context.Request.Headers.ContentType.ToString();
        if (!contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            await context.Response.WriteAsync("Unsupported content type");
            _logger.LogError("Unsupported content type");
            return;
        }

        if (context.Request.Path.StartsWithSegments("/api/devices") &&
            (context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) || 
             context.Request.Method.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
        {
            context.Request.EnableBuffering();

            string body;
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            DeviceCreateRequest? device;
            try
            {
                device = JsonSerializer.Deserialize<DeviceCreateRequest>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid format.");
                _logger.LogError(ex, "Failed to deserialize DeviceCreateRequest.");
                return;
            }

            if (device == null)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Device object is null.");
                _logger.LogError("Device object is null.");
                return;
            }

            var errors = new List<string>();
            string deviceType = GetDeviceTypeName(device.TypeId);

            var matchingRules = _validationRules
                .Where(rule =>
                {
                    if (string.IsNullOrWhiteSpace(rule.PreRequestName) || string.IsNullOrWhiteSpace(rule.PreRequestValue))
                        return false;

                    var prop = typeof(DeviceCreateRequest).GetProperty(rule.PreRequestName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var value = prop?.GetValue(device)?.ToString();

                    return rule.Type.Equals(deviceType, StringComparison.OrdinalIgnoreCase)
                        && value != null
                        && value.Equals(rule.PreRequestValue, StringComparison.OrdinalIgnoreCase);
                })
                .SelectMany(r => r.Rules)
                .ToList();

            foreach (var rule in matchingRules)
            {
                if (!device.AdditionalProperties.TryGetProperty(rule.ParamName, out var propValue))
                {
                    errors.Add($"Missing obligatory parameter: {rule.ParamName}");
                    continue;
                }

                var pattern = rule.GetCleanRegex();
                if (!string.IsNullOrEmpty(pattern))
                {
                    if (!Regex.IsMatch(propValue.ToString(), pattern))
                        errors.Add($"{rule.ParamName} doesn't match pattern: {pattern}");
                }
                else if (rule.RegexList?.Any() == true)
                {
                    if (!rule.RegexList.Contains(propValue.ToString()))
                        errors.Add($"{rule.ParamName} must be included in: {string.Join(", ", rule.RegexList)}");
                }
            }

            if (errors.Any())
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { errors });
                _logger.LogError("Validation failed: {Errors}", string.Join("; ", errors));
                return;
            }
        }

        _logger.LogInformation("Finished using Middleware");
        await _next(context);
    }

    private string GetDeviceTypeName(int? typeId)
    {
        return typeId switch
        {
            1 => "PC",
            2 => "Embedded",
            3 => "Smartwatch",
            _ => "Unknown"
        };
    }
}
