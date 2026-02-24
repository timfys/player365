using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using SmartWinners.MetaData;

namespace SmartWinners.Helpers;

public class MasterDataManger
{
    public static List<Countries> Countries { get; }

    static MasterDataManger()
    {
        Countries = GetCountriesList();
    }

    private static List<Countries> GetCountriesList()
    {
    using var reader = new StreamReader($"{EnvironmentHelper.Environment.WebRootPath}/MetaData/country-codes.csv");
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
    return [.. csv.GetRecords<Countries>()];
  }
}