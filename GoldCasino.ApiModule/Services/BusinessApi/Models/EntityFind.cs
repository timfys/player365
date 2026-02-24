namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityFind
{
	public int? LimitFrom { get; set; }
	public int? LimitCount { get; set; }
	public bool? LimitEntitiesPerBusiness { get; set; }
	public string[]? Fields { get; set; }
	public Dictionary<string, string>? Filter { get; set; }
}

public class EntityFindResponse : ApiResponse
{
	public List<Entity> Entities { get; init; } = [];
}

public class EntityFindResult
{
	public List<Entity> Entities { get; init; } = [];
}

public class EntityFindError
{
	public int ResultCode { get; set; }
	public string ResultMessage { get; set; } = "";
}	