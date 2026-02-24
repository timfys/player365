using CsvHelper.Configuration.Attributes;

namespace SmartWinners.MetaData;

public class Countries
{
    [Name("CLDR display name")]
    public string CountryName { get; set; }
    
    [Name("Dial")]
    public string CallingCode { get; set; }
    [Name("ISO3166-1-Alpha-2")]
    public string CountryIso { get; set; }
    [Name("ISO4217-currency_alphabetic_code")]
    public string CurrencyIso { get; set; }
}