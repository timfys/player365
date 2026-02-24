using BusinessApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartWinners.Helpers;

public static class CurrencyHelper
{
    public static CurrencyResponse GetCurrency()
    {
        return new()
        {
            CurrencyId = 1,
            Currency = "United States (USD)",
            Symbol = "$",
            ExchangeRate = 1,
            CurrencyCode = "USD"
        };
    }

    public static async Task<List<CurrencyDetails>> GetCurrencies()
    {
        if (WebStorageUtility.TryGetString(WebStorageUtility.CurrenciesList, out var value))
        {
            var currencyJson = CompressUtility.DecompressString(Convert.FromBase64String(value));

            return JsonConvert.DeserializeObject<List<CurrencyDetails>>(currencyJson);
        }

        var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

        var apiRequest = new General_DataList_GetRequest
        {
            TableName = "currency"
        };

        var apiResponse = await client.General_DataList_GetAsync(apiRequest);

        WebStorageUtility.SetString(WebStorageUtility.CurrenciesList,
            Convert.ToBase64String(CompressUtility.CompressString(apiResponse.@return)),
            WebStorageUtility.GetUserDateTime() + TimeSpan.FromDays(1));

        return JsonConvert.DeserializeObject<List<CurrencyDetails>>(apiResponse.@return);
    }
}

public class CurrencyResponse
{
    public int CurrencyId { get; set; }

    public string Currency { get; set; }

    public string Symbol { get; set; }

    public decimal ExchangeRate { get; set; }

    public string CurrencyCode { get; set; }
}

public class CountryResponse
{
    public int CountryId { get; set; }

    public string CountryName { get; set; }

    public string CallingCode { get; set; }

    public string CurrencyId { get; set; }

    public string CurrencyCode { get; set; }

    public string ISO { get; set; }

    public string Language { get; set; }
}

public class CurrencyDetails
{
    public decimal ExchangeRate { get; set; }

    public string Symbol { get; set; }

    public string CountryIso { get; set; }

    [JsonProperty("CurrencyCode")] public string CurrencyIso { get; set; }
}