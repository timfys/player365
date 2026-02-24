using SmartWinners.Configuration;

namespace SmartWinners.Models;

	public class StripeConfiguration : MyConfiguration
	{
		public string SecretKey { get; set; }
		public string PublishableKey { get; set; }
		public string WebhookSecret { get; set; }
	}
