using GoldCasino.ApiModule.Common;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public sealed class EntityLogTraffic
{
    public int? EntityId { get; set; }
    public DeviceKind DeviceKind { get; set; } = DeviceKind.Web;
    public string SystemInfo { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Fields { get; set; }
}

internal sealed class EntityLogTrafficResponse : ApiResponse
{
}
