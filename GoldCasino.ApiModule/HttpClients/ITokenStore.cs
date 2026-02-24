public interface ITokenStore
{
	string? Current { get; }
	void Set(string token);
}
