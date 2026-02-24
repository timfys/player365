using System.ComponentModel.DataAnnotations;

namespace GoldCasino.ApiModule.Configuration;

/// <summary>
/// Configuration options for Plisio payment gateway
/// </summary>
public class PlisioOptions
{
    /// <summary>
    /// Plisio API base URL
    /// </summary>
    [Required]
    public string BaseUrl { get; set; } = "https://api.plisio.net/api/v1/";

    /// <summary>
    /// Plisio API secret key for authentication and webhook signature verification
    /// </summary>
    [Required]
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// Default invoice expiration time in minutes
    /// </summary>
    public int DefaultExpireMinutes { get; set; } = 60;

    /// <summary>
    /// Comma-separated list of allowed cryptocurrencies (e.g., "BTC,ETH,USDT_TRX")
    /// If empty, all active currencies from Plisio account settings will be available
    /// </summary>
    public string? AllowedCryptocurrencies { get; set; }

    /// <summary>
    /// Custom callback host for webhook URLs (e.g., ngrok tunnel URL).
    /// If null or empty, the request host will be used.
    /// </summary>
    public string? CallbackUrl { get; set; }
}
