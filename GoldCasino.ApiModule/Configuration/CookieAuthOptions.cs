namespace GoldCasino.ApiModule.Configuration;

public class CookieAuthOptions
{
	public int CurrentKeyVersion { get; set; }
	public int SchemaVersion { get; set; }
	public Dictionary<int, string> Keys { get; set; } = [];
}
