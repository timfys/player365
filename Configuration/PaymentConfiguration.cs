namespace SmartWinners.Configuration;

public class PaymentConfiguration : MyConfiguration
{
    public int PaymentId { get; set; }

    public string CurrencyIso { get; set; }
}