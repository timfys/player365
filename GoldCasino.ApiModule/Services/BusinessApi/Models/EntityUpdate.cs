namespace GoldCasino.ApiModule.Services.BusinessApi.Models;
public sealed class EntityUpdate
{
	public int EntityId { get; init; }
	public Dictionary<string, string> Fields { get; init; } = [];
	public Dictionary<string, string>? ImageFields { get; init; }
}
	
public sealed class EntityUpdateResult
{
}

public sealed class EntityUpdateResponse : ApiResponse
{
	  public const int ResultCodeEmailNotValid = -498394;
		public const int ResultCodeExists = -475;
    public bool IsEmailNotValid() => ResultCode == ResultCodeEmailNotValid;
    public bool IsEmailExists() => ResultCode == ResultCodeExists && ResultMessage?.Contains("email", StringComparison.OrdinalIgnoreCase) == true;
		public bool IsMobileExists() => ResultCode == ResultCodeExists && ResultMessage?.Contains("mobile", StringComparison.OrdinalIgnoreCase) == true;
}
