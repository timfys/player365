using GoldCasino.ApiModule.Configuration;
using Microsoft.Extensions.Options;

namespace Plisio.ApiClient;

/// <summary>
/// Partial class to extend the NSwag-generated PlisioApiClient with API key authentication
/// </summary>
public partial class PlisioApiClient
{
    private PlisioOptions? _plisioOptions;

    /// <summary>
    /// Sets the Plisio options for API key injection
    /// </summary>
    public void SetOptions(IOptions<PlisioOptions> options)
    {
        _plisioOptions = options?.Value;
    }

    /// <summary>
    /// Prepare the request by adding the API key as a query parameter
    /// </summary>
    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
    {
        if (_plisioOptions != null && !string.IsNullOrEmpty(_plisioOptions.SecretKey))
        {
            var uriBuilder = new UriBuilder(client.BaseAddress + url);
            var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
            query["api_key"] = _plisioOptions.SecretKey;
            uriBuilder.Query = query.ToString();
            request.RequestUri = uriBuilder.Uri;
        }
    }

    /// <summary>
    /// Prepare the request by adding the API key as a query parameter (StringBuilder overload)
    /// </summary>
    partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder)
    {
        if (_plisioOptions != null && !string.IsNullOrEmpty(_plisioOptions.SecretKey))
        {
            if (urlBuilder.Length > 0)
            {
                var separator = urlBuilder.ToString().Contains('?') ? "&" : "?";
                urlBuilder.Append($"{separator}api_key={Uri.EscapeDataString(_plisioOptions.SecretKey)}");
            }
        }
    }
}
