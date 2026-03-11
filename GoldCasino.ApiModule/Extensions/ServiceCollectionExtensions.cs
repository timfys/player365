using GoldCasino.ApiModule.Configuration;
using GoldCasino.ApiModule.HttpClients;
using GoldCasino.ApiModule.HttpClients.Lvslot;
using GoldCasino.ApiModule.Integrations.BusinessApi;
using GoldCasino.ApiModule.Integrations.PlayerClub365;
using GoldCasino.ApiModule.Services;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.PlayerClub365Api;
using GoldCasino.ApiModule.Services.SmartWinnersApi;
using Microsoft.Extensions.Configuration;
using PalaceCasino.Agent.Client;
using Plisio.ApiClient;
using System.Net.Http.Headers;

namespace GoldCasino.ApiModule.Extensions;
public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApiModule(this IServiceCollection services, IConfiguration cfg)
	{
		services.AddHttpContextAccessor();
		var prodUrl = "www.playerclub365.com";
		// ---------- Options ----------
		services.AddOptions<GoldSlotApiOptions>()
			.BindConfiguration("GoldSlotApi")
			.ValidateDataAnnotations()
			.ValidateOnStart()
			.Configure<ITokenStore>((opts, ts) =>
			{
				if (!string.IsNullOrWhiteSpace(opts.AccessToken))
					ts.Set(opts.AccessToken!);
			});

		services.AddOptions<LvslotApiOptions>()
			.BindConfiguration("LvslotApi")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddOptions<CookieAuthOptions>()
			.BindConfiguration("CookieAuth")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddOptions<Playerclub365Options>()
			.Configure<IHttpContextAccessor, IConfiguration>((opts, accessor, cfg) =>
			{
				var request = accessor.HttpContext?.Request;
				var fullUrl = $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}{request.Path.Value}{request.QueryString.Value}";
				Console.WriteLine($"test2 {fullUrl}");
				var section = fullUrl.Contains(prodUrl) ? "Playerclub365Live" : "Playerclub365";
				cfg.GetSection(section).Bind(opts);
			});
		services.AddOptions<SmartWinnersOptions>()
			.Configure<IHttpContextAccessor>((opts, accessor) =>
			{
				var request = accessor.HttpContext?.Request;
				var fullUrl = $"{request?.Scheme}://{request?.Host.Value}{request?.PathBase.Value}{request?.Path.Value}{request?.QueryString.Value}";
				Console.WriteLine($"test3 {fullUrl}");
				var section = fullUrl.Contains(prodUrl) ? "SmartWinnersLive" : "SmartWinners";
				cfg.GetSection(section).Bind(opts);
			});

		services.AddOptions<BusinessApiOptions>()
			.Configure<IHttpContextAccessor>((opts, accessor) =>
			{
				var request = accessor.HttpContext?.Request;
				var fullUrl = $"{request?.Scheme}://{request?.Host.Value}{request?.PathBase.Value}{request?.Path.Value}{request?.QueryString.Value}";
				Console.WriteLine($"test4 {fullUrl}");
				var section = fullUrl.Contains(prodUrl) ? "BusinessAPILive" : "BusinessAPI";
				cfg.GetSection(section).Bind(opts);
			});


		// ---------- SOAP clients ----------
		services.AddSoapClient<IPlayerclub365, Playerclub365Options>()
						.AddSoapClient<ISmartWinners, SmartWinnersOptions>()
						.AddSoapClient<IBusinessAPI, BusinessApiOptions>();

		// ---------- Http clients ----------
		services.AddSingleton<ITokenStore, InMemoryTokenStore>();
		services.AddTransient<BearerTokenHandler>();

		services.AddHttpClient("PalaceCasino", (sp, http) =>
		{
			var opts = sp.GetRequiredService<IOptions<GoldSlotApiOptions>>().Value;
			http.BaseAddress = new Uri(opts.BaseUrl!);
			http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		})
		.AddHttpMessageHandler<BearerTokenHandler>()
		.AddTypedClient<IGoldSlotApiClient>((http, sp) =>
		{
			var opts = sp.GetRequiredService<IOptions<GoldSlotApiOptions>>().Value;
			return new GoldSlotApiClient(opts.BaseUrl!, http);
		});

		services.AddHttpClient<LvslotApiClient>((sp, client) =>
		{
			var opts = sp.GetRequiredService<IOptions<LvslotApiOptions>>().Value;
			client.BaseAddress = new Uri(opts.BaseUrl);
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		// Plisio API client
		services.AddOptions<PlisioOptions>()
			.BindConfiguration("Plisio")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		services.AddHttpClient<IPlisioApiClient, PlisioApiClient>("Plisio", (sp, client) =>
		{
			var opts = sp.GetRequiredService<IOptions<PlisioOptions>>().Value;
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		})
		.AddTypedClient<IPlisioApiClient>((http, sp) =>
		{
			var opts = sp.GetRequiredService<IOptions<PlisioOptions>>();
			var plisioClient = new PlisioApiClient(http);
			plisioClient.SetOptions(opts);
			return plisioClient;
		});

		// ---------- Core DI ----------
		services.AddScoped<IBusinessApiService, BusinessApiService>();
		services.AddScoped<ISmartWinnersApiService, SmartWinnersApiService>();
		services.AddScoped<IPlayerClub365ApiService, PlayerClub365ApiService>();
		services.AddSingleton<ISupportedLanguagesService, SupportedLanguagesService>();
		services.AddScoped<AuthService>();
		services.AddScoped<UserService>();

		
		services.AddValidation();

		// ---------- Authentication (custom scheme only, no defaults) ----------
		services.AddEncryptedCookieAuth(cfg);

		return services;
	}
}
