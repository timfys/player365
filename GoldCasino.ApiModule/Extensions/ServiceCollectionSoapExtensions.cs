using Microsoft.Extensions.Hosting;
using System.ServiceModel;

namespace GoldCasino.ApiModule.Extensions;

public static class ServiceCollectionSoapExtensions
{
	/// <summary>
	/// Register ChannelFactory<TContract>; (singleton) and channel itself (scoped).
	/// </summary>
	public static IServiceCollection AddSoapClient<TContract, TOpt>(
			this IServiceCollection services,
			Action<BasicHttpBinding>? tune = null) // optionally tune binding
			where TContract : class
			where TOpt : SoapOptionsBase, new() // SmartWinnersOptions, Playerclub365Options …
	{
		services.AddSingleton(sp =>
		{
			var opt = sp.GetRequiredService<IOptions<TOpt>>().Value;
			var tm = opt.Timeouts;

			var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
			{
				OpenTimeout = TimeSpan.FromSeconds(tm!.Open),
				SendTimeout = TimeSpan.FromSeconds(tm.Send),
				ReceiveTimeout = TimeSpan.FromSeconds(tm.Receive),
				MaxReceivedMessageSize = 64 * 1024 * 1024
			};

			//binding.TextEncoding = Encoding.GetEncoding("us-ascii");
			//tune?.Invoke(binding);                      // if necessary SSL, MTOM etc.

			var endpoint = new EndpointAddress(((dynamic)opt).Endpoint);
			return new ChannelFactory<TContract>(binding, endpoint);
		});

		// 3. Proxy channel ― scoped
		services.AddScoped(sp =>
    {
        var factory = sp.GetRequiredService<ChannelFactory<TContract>>();
        var proxy = factory.CreateChannel();
        
        // The DI container automatically calls Dispose() on this proxy 
        // when the HTTP request finishes.
        return proxy;
    });


		return services;
	}
}
