using BusinessApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using File = System.IO.File;

namespace SmartWinners.Helpers;

	public static class CountryUtility
	{
		private static readonly object _sync = new();
		private static List<Country>? _countries { get; set; } = null;

		private static Country[] GetListOfCountries()
		{
			var client = EnvironmentHelper.BusinessApiConfiguration.InitClient();

			var apiRequest = new General_DataList_GetRequest
			{
				TableName = "countries",
				FilterFields = new[] { "Order by" },
				FilterValues = new[] { "LastDateUsed desc, CountryName" }
			};

			var apiResponse = client.General_DataList_GetAsync(apiRequest).GetAwaiter().GetResult();

			var countries = JsonConvert.DeserializeObject<List<Country>>(apiResponse.@return) ?? new List<Country>();

			return [.. countries.Where(x => !x.Name.Equals("country", StringComparison.OrdinalIgnoreCase))];
		}

		public static Country GetCountryByAlpha2(string alpha2)
			=> Countries.FirstOrDefault(x => x.Alpha2 == alpha2.ToUpper());

		public static List<Country> Countries
		{
			get
			{
				if (ExpireTime is null || ExpireTime < DateTime.UtcNow || _countries is null)
				{
					lock (_sync)
					{
						if (ExpireTime is null || ExpireTime < DateTime.UtcNow || _countries is null)
						{
							var path = $"{EnvironmentHelper.Environment.WebRootPath}/countries.json";
							List<Country>? pure = null;
							try
							{
								if (File.Exists(path))
								{
									pure = JsonConvert.DeserializeObject<List<Country>>(File.ReadAllText(path));
								}
							}
							catch
							{
								pure = null;
							}

							if (pure == null || pure.Count == 0)
							{
								try
								{
									pure = [.. GetListOfCountries()];

									if (pure.Count > 0)
									{
										File.WriteAllText(path, JsonConvert.SerializeObject(pure));
									}
								}
								catch
								{
									pure = pure ?? new List<Country>();
								}
							}

							_countries = new List<Country>
							{
								new Country { Alpha2 = "empty", Code = "" }
							};
							if (pure is { Count: > 0 })
								_countries.AddRange(pure);

							ExpireTime = DateTime.UtcNow + TimeSpan.FromMinutes(EnvironmentHelper.SeoConfiguration.RamCacheTime);
						}
					}
				}

				return _countries!;
			}
		}

		public static DateTime? ExpireTime { get; set; }

		public class Country
		{
			[JsonProperty("CountryName")] public string Name;
			[JsonProperty("ISO3166")] public string Alpha2;
			[JsonProperty("CallingCode")] public string Code;
			[JsonProperty("LastDateUsed")] public DateTime? LastDateUsed;
			[JsonProperty("CurrencyCode")] public string CurrencyCode;
		}
	}
