namespace SmartWinners.Configuration;

public class StripeConfiguration : MyConfiguration
{
    public string SecretKey { get; set; }
    
    public string PublicKey { get; set; }
    public string WebhookSecret { get; set; }
}