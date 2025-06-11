using System.Text.Json.Serialization;

namespace API.Helpers.Middleware;

public class ParamRule
{
    public string ParamName { get; set; }

    [JsonPropertyName("regex")]
    public string Regex { get; set; }

    [JsonPropertyName("regex:")]
    public string RegexWithColon { get; set; }

    public List<string> RegexList { get; set; }

    public string GetCleanRegex()
    {
        var raw = Regex ?? RegexWithColon;
        if (string.IsNullOrEmpty(raw)) return null;

        return raw.Trim('/');
    }
}
