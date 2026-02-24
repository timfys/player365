namespace GoldCasino.ApiModule.Configuration;

public class SoapOptionsBase
{
    public string Endpoint { get; set; } = "";
    public SoapTimeout Timeouts { get; set; } = new();
}
