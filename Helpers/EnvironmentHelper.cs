using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SmartWinners.Configuration;
using StripeConfiguration = SmartWinners.Configuration.StripeConfiguration;

namespace SmartWinners.Helpers;

public static class EnvironmentHelper
{
	public static IWebHostEnvironment Environment { get; set; }

	public static bool IsClub
	{
		get
		{
			try
			{
				return DomainsHelper.GetDomainNameType() is DomainType.PlayerClub or DomainType.PlayerClubTest;
			}
			catch (Exception e)
			{
				return false;
			}

		}
	}

	public static string ConfigName
	{
		get
		{
			//var context = HttpContextAccessor.HttpContext;
			//return context.Request.Host.Value.Contains("beta") || context.Request.Host.Value.Contains("localhost") ? "ApiConfig.json" : "ApiConfig-Live.json";
			return "ApiConfig.json";

		}
	}


	public static SmartWinnersApiConfiguration _smartWinnersApiConfiguration;
	public static CasinoGamesApiConfiguration _casinoGamesApiConfiguration;
	public static BusinessApiConfiguration _businessApiConfiguration;
	public static WhatsAppConfiguration _whatsAppConfiguration;
	public static TelegramConfiguration _telegramConfiguration;
	public static StripeConfiguration _stripeConfiguration;
	public static SeoConfiguration _seoConfiguration;

	public static SmartWinnersApiConfiguration SmartWinnersApiConfiguration
	{
		get
		{
			if (_smartWinnersApiConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<SmartWinnersApiConfiguration>(ConfigName, out var config);

				_smartWinnersApiConfiguration = config;
			}

			return _smartWinnersApiConfiguration;
		}
	}

	public static CasinoGamesApiConfiguration CasinoGamesApiConfiguration
	{
		get
		{
			if (_casinoGamesApiConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<CasinoGamesApiConfiguration>(ConfigName, out var config);

				_casinoGamesApiConfiguration = config;
			}

			return _casinoGamesApiConfiguration;
		}
	}
	public static WhatsAppConfiguration WhatsAppConfiguration
	{
		get
		{
			if (_whatsAppConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<WhatsAppConfiguration>(ConfigName, out var config);

				_whatsAppConfiguration = config;
			}

			return _whatsAppConfiguration;
		}
	}
	public static TelegramConfiguration TelegramConfiguration
	{
		get
		{
			if (_telegramConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<TelegramConfiguration>(ConfigName, out var config);

				_telegramConfiguration = config;
			}

			return _telegramConfiguration;
		}
	}
	public static StripeConfiguration StripeConfiguration
	{
		get
		{
			if (_stripeConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<StripeConfiguration>(ConfigName, out var config);

				_stripeConfiguration = config;
			}

			return _stripeConfiguration;
		}
	}
	public static BusinessApiConfiguration BusinessApiConfiguration
	{
		get
		{
			if (_businessApiConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<BusinessApiConfiguration>(ConfigName, out var config);

				_businessApiConfiguration = config;
			}

			return _businessApiConfiguration;
		}
	}

	public static SeoConfiguration SeoConfiguration
	{
		get
		{
			if (_seoConfiguration is null)
			{
				ConfigReader.ReadFromJsonConfig<SeoConfiguration>(ConfigName, out var config);

				_seoConfiguration = config;
			}

			return _seoConfiguration;
		}
	}
	
	public static IHttpContextAccessor HttpContextAccessor { get; set; }

	public static DateTime RequestStartTime { get; set; }
}

public class MySessionObject : IDisposable
{
	public Dictionary<string, string> Values { get; set; }

	private bool _disposed = false;

	// Your class implementation goes here

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				Values.Clear();

				// Dispose managed resources here
			}

			// Dispose unmanaged resources here

			_disposed = true;
		}
	}

	~MySessionObject()
	{
		Dispose(false);
	}
}