namespace GoldCasino.ApiModule.Configuration;

public class SoapTimeout
{
	public double Open { get; set; } = 1.8;
	public double Send { get; set; } = 1.8;
	public double Receive { get; set; } = 1.8;
}
