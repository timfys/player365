namespace GoldCasino.ApiModule.HttpClients;

public sealed class InMemoryTokenStore : ITokenStore
{
	private volatile string? _token;
	public string? Current => _token;
	public void Set(string token) => _token = token;
}