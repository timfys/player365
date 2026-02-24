using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public enum DeviceKind
{
    Android = 1,
    Ios = 2,
    WindowsPhone = 3,
    Web = 4
}

public class OlLogin
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string IP { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public DeviceKind DeviceKind { get; set; } = DeviceKind.Web;
}

public class OlLoginResult
{
    public string EntityId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Lid { get; set; } = string.Empty;
}

// Raw response shape returned by SOAP as JSON
internal class OlLoginResponse : ApiResponse
{
    public int EntityId { get; set; }

    [JsonPropertyName("lid")]
    public string? Lid { get; set; }
}
