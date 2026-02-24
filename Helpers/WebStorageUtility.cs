using BusinessApi;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartWinners.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace SmartWinners.Helpers;

	public static class WebStorageUtility
	{
		public static readonly DateTime DeletedCookieDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public static readonly DateTime LifetimeCookieDate = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public const string UserValueName = "eb82c890b940475284174f68442b4ccf";
		public const string UserBalance = "jhvcn28734rtc232x983rbb9udcfyif6d";
		public const string UserBonusBalance = "kh83n29734rtc232x983rbb9udc8yif7e";
		public const string CurrencyValueName = "fsoafnenfwonoiwinpewp434r34";
		public const string CurrencyObject = "5435243gertgieonoisgt54eergd5";
		public const string PaymentSuccessObjLottery = "gbg54y456b9nvu455n59";
		public const string PaymentSuccessObjSyndicate = "gbg54y136b9nvu455n59";
		public const string AffiliateIdCookieName = "aid";
		public const string UserApiAccessData = "iuesrof847tb39ft784bfet5eef5y5";
		public const string GiftChecked = "540ifnto549ny84v095rn8n";
		public const string TimeZone = "rofnurntsot4895nd5t34org65yg";
		public const string CurrenciesList = "ersftfmdi54np509y4n56p9gyr5";
		public const string LangIso = "ino4pf3t3495fn9y48ynf455y6n";
		public const string CurrencyCultureId = "5f9no34968u3n95698y465inf";
		public const string SelectedVoucherId = "5f9no34968u3n95698y465inff435ioun";
		public const string VisitedBeforeSignUp = "45igjyf5496yh5486y0j5846jf5465";
		public const string SignInPageIndex = "mgeiong5o945ngt94g654g4";
		public const string FailedChargeSentByEmail = "43958hft347968yuon65tufhy";
		public const string TimeZoneOffset = "4iu5ny64596ogyn5u6n7gu5no";
		public const string AfterSignUpFlag = "9u45ftn4598ny43895n49856yn4";
		public const string ExternalReferer = "8g4n5t9f4n85yg94n85gn49t5";
		public const string AfterPaymentFlag = "5t43t54t34t346363654g546g";
		public const string AffiliateScriptControl = "8u5btg4o8bgy854g8n6y89";
		public const string LastUsedCurrencyIso = "dofignruigner89rtonget8vne8";
		public static string CurrentLangIso;

		public static void SignOut(HttpContext context)
		{
			RemoveValue(CurrencyObject);
			RemoveValue(UserValueName);
			RemoveValue(UserApiAccessData);
			RemoveValue(UserBalance);
			RemoveValue(UserBonusBalance);
			//RemoveValue(TimeZone);
			RemoveValue(PaymentSuccessObjSyndicate);
			RemoveValue(PaymentSuccessObjLottery);
			RemoveValue(GiftChecked);
			RemoveValue(SelectedVoucherId);
			RemoveValue(VisitedBeforeSignUp);
			RemoveValue(LastUsedCurrencyIso);
			RemoveValue("Lid");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="returnNull">Indicates whether to return a null if there is no user date time found or server date time</param>
		/// <returns>Return user date time or server date time otherwise if there is no info about user timezone</returns>
		public static DateTime? GetUserDateTime(bool? returnNull = null)
		{
			var userTimeZone = GetUserTimeZone();

			if (userTimeZone is null)
				return returnNull.HasValue && returnNull.Value ? null : DateTime.Now;

			return DateTime.UtcNow + userTimeZone.BaseUtcOffset;
		}

		public static TimeZoneInfo GetUserTimeZone()
		{
			if (TryGetString(TimeZone, out var timeZoneId))
			{
				var userTimezone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(x => x.Id.Equals(timeZoneId));

				return userTimezone;
			}

			return TimeZoneInfo.Utc;
		}

		public static UserBalanceInfo GetUserBalance(HttpContext context)
		{
			return CryptoUtility.DecryptObject<UserBalanceInfo>(GetCookieValue(context, UserBalance));
		}

		public static decimal? GetUserBonusBalance(HttpContext context)
		{
			var value = GetCookieValue(context, UserBonusBalance);
			if (string.IsNullOrEmpty(value)) return null;
			var decrypted = CryptoUtility.DecryptObject<UserBonusBalanceInfo>(value);
			return decrypted?.BonusBalance;
		}

		public static User? GetSignedUser(HttpContext context = null)
		{
			return null;
		}

		public static object GetEntityField(int entityId, string customField)
		{
			var config = EnvironmentHelper.BusinessApiConfiguration;

			var client = config.InitClient();

			var apiRequest = new Entity_FindRequest
			{
				ol_EntityId = config.ol_EntityId,
				ol_UserName = config.ol_UserName,
				ol_Password = config.ol_Password,
				BusinessId = config.BusinessId,
				FilterFields = new[] { "EntityId" },
				FilterValues = new[] { $"{entityId}" },
				Fields = new[] { customField }
			};

			var entry = JsonConvert.DeserializeObject<List<object>>(client.Entity_Find(apiRequest).@return).FirstOrDefault() as IEnumerable<KeyValuePair<string, JToken>>;

			if (entry is null)
				return null;

			var value = entry.FirstOrDefault(x => x.Key.Equals(customField, StringComparison.OrdinalIgnoreCase)).Value.Value<string>();

			return value;
		}


		public static CurrencyDetails? GetUserCurrencyDetails(HttpContext? context = null, bool ignoreClub = false)
		{
			if (EnvironmentHelper.IsClub && !ignoreClub)
			{
				return new CurrencyDetails
				{
					CountryIso = "USA",
					Symbol = "$",
					CurrencyIso = "USD",
					ExchangeRate = 1
				};
			}

			context ??= EnvironmentHelper.HttpContextAccessor.HttpContext;

			if (context.Items.TryGetValue(CurrencyObject, out var currencyObj) && currencyObj is not null)
				return currencyObj as CurrencyDetails;

			if (TryGetObject<CurrencyDetails>(CurrencyObject, out var currencySt))
				return currencySt;

			if (context.Request.Host.Value.Contains("iwin.co.il") ||
					context.Request.Host.Value.Contains("smartwinners.co.il"))
			{
				var heCurrency = CurrencyHelper.GetCurrency();
				return new CurrencyDetails
				{
					CountryIso = "IL",
					Symbol = heCurrency.Symbol,
					CurrencyIso = heCurrency.CurrencyCode,
					ExchangeRate = heCurrency.ExchangeRate
				};
			}
			else
			{
				if ("he".Equals(GetUserLangIso(), StringComparison.OrdinalIgnoreCase))
				{
					var currency = new CurrencyDetails
					{
						CountryIso = "IL",
						Symbol = "₪",
						CurrencyIso = "ILS",
						ExchangeRate = CurrencyHelper.GetCurrency().ExchangeRate
					};

					context.Items.Add(CurrencyObject, currency);

					return currency;
				}

				var user = GetSignedUser();

				if (user is not null)
				{
					var currency = CurrencyHelper.GetCurrency();

					return new CurrencyDetails
					{
						CountryIso = user.Country.ToUpper(),
						Symbol = currency.Symbol,
						CurrencyIso = currency.CurrencyCode,
						ExchangeRate = currency.ExchangeRate
					};
				}

				return new CurrencyDetails
				{
					CountryIso = "USA",
					Symbol = "$",
					CurrencyIso = "USD",
					ExchangeRate = 1
				};
			}
		}

		public static int GetAffiliateId(HttpContext? context = null)
		{
			if (context is null)
				context = EnvironmentHelper.HttpContextAccessor.HttpContext;
			TryGetString(AffiliateIdCookieName, out var aIdStr);
			var aid = int.TryParse(aIdStr, out var result) ? result : 0;
			var user = GetSignedUser();

			if (aid == 0 && user is not null && user.AffiliateId > 0)
			{
				aid = user.AffiliateId;
			}

			return aid;
		}

		public static void SetAffiliateId(HttpContext context, int value)
		{
			SetCookie(context, AffiliateIdCookieName, value.ToString());
		}


		public static void SetCookie(HttpContext context, string name, string value, DateTime? expire = null)
		{
			var cookies = context.Response.Cookies;
			cookies.Delete(name);
			if (value is not null)
				cookies.Append(name, value ?? "", new CookieOptions
				{
					Path = "/",
					Expires = expire.HasValue ? expire : (value != null ? LifetimeCookieDate : DeletedCookieDate)
				});
		}


		private static string GetCookieValue(HttpContext context, string name)
				=> context.Request.Cookies[name];

		public static string? GetUserLangIso(bool? forUrl = null)
		{
			if (TryGetString(LangIso, out var iso) || !string.IsNullOrEmpty(CurrentLangIso))
			{

				iso = string.IsNullOrEmpty(CurrentLangIso) ? iso : CurrentLangIso;

				if (forUrl is null)
				{
					return iso;
				}

				/*var context = EnvironmentHelper.HttpContextAccessor.HttpContext;

				if (context.Request.Host.Value.Contains("smartwinners.co.il") ||
						context.Request.Host.Value.Contains("iwin.co.il") || true)
				{
						return iso.Equals("he") ? "" : $"/{iso}";
				}*/

				return iso.Equals("en") ? "" : $"/{iso}";
			}

			return iso;
		}

		public static string? RewriteUrlWithUserIso(string pageUrl)
		{
			var iso = GetUserLangIso();

			if (iso is null or "")
			{
				return pageUrl.StartsWith("/") ? pageUrl : $"/{pageUrl}";
			}
			else
			{
				iso = iso is "en" ? "" : $"/{iso}";

				if (iso == "")
				{
					return pageUrl;
				}

				if (pageUrl.StartsWith(iso, StringComparison.OrdinalIgnoreCase))
				{
					return pageUrl;
				}

				return pageUrl.StartsWith("/") ? $"{iso}{pageUrl}" : $"{iso}/{pageUrl}";
			}
		}

		public static CultureInfo GetUserCultureInfo()
		{
			TryGetString(CurrencyCultureId, out var cultureIdStr);

			if (int.TryParse(cultureIdStr, out var cultureId))
			{
				return CultureInfo.GetCultures(CultureTypes.AllCultures)
						.First(x => x.LCID == cultureId);
			}

			return Thread.CurrentThread.CurrentCulture;
		}


		/// <summary>
		/// Use this method to check both cookie storage and then session storage
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="isEncrypted"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static bool TryGetObject<T>(string key, out T value, bool? isEncrypted = null) where T : class
		{
			value = null;
			try
			{
				var valueStr = GetCookieValue(EnvironmentHelper.HttpContextAccessor.HttpContext, key);

				if (isEncrypted.HasValue && isEncrypted.Value && !string.IsNullOrEmpty(valueStr))
				{
					value = CryptoUtility.DecryptObject<T>(valueStr);
				}
				else if (!string.IsNullOrEmpty(valueStr))
				{
					value = JsonConvert.DeserializeObject<T>(valueStr);
				}

				return value is not null;
			}
			catch
			{
				return value is not null;
			}
		}

		/// <summary>
		/// Use this method to check both cookie storage and then session storage
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="isEncrypted"></param>
		/// <returns></returns>
		public static bool TryGetString(string key, out string value, bool? isEncrypted = null)
		{
			try
			{
				value = GetCookieValue(EnvironmentHelper.HttpContextAccessor.HttpContext, key);

				var result = string.IsNullOrEmpty(value);

				if (isEncrypted.HasValue && isEncrypted.Value && !result)
				{
					value = CryptoUtility.DecryptString(value);
					result = string.IsNullOrEmpty(value);
				}

				return !result;
			}
			catch
			{
				value = string.Empty;
				return false;
			}
		}


		public static string GetSessionId()
		{
			return "";
		}

		public static void RemoveValue(string key)
		{
			var context = EnvironmentHelper.HttpContextAccessor.HttpContext;

			context.Response.Cookies.Delete(key);
		}

		public static void SetString(string key, string value, DateTime? expire = null)
		{
			SetCookie(EnvironmentHelper.HttpContextAccessor.HttpContext, key, value, expire);
		}

		public static void EndRequest()
		{
			CurrentLangIso = null;
			EnvironmentHelper._businessApiConfiguration = null;
			EnvironmentHelper._smartWinnersApiConfiguration = null;
			EnvironmentHelper._seoConfiguration = null;
			EnvironmentHelper._telegramConfiguration = null;
			EnvironmentHelper._whatsAppConfiguration = null;
			EnvironmentHelper._stripeConfiguration = null;
		}
	}