using System;
using System.Threading.Tasks;
using IPinfo;
using Microsoft.Extensions.Logging;

namespace SmartWinners.Services;

public interface IIpInfoCountryResolver
{
    Task<string?> GetCountryIsoAsync(string? ipAddress);
}

public class IpInfoCountryResolver(IPinfoClient client, ILogger<IpInfoCountryResolver> logger) : IIpInfoCountryResolver
{
    public async Task<string?> GetCountryIsoAsync(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)
            || string.Equals(ipAddress, "::1", StringComparison.Ordinal)
            || string.Equals(ipAddress, "127.0.0.1", StringComparison.Ordinal)
            || string.Equals(ipAddress, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            var response = await client.IPApi.GetDetailsAsync(ipAddress);
            return string.IsNullOrWhiteSpace(response?.Country) ? null : response.Country.ToLowerInvariant();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Unable to resolve country for IP {IpAddress}", ipAddress);
            return null;
        }
    }
}
