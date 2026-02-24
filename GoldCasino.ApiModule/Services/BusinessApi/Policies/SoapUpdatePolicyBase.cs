namespace GoldCasino.ApiModule.Services.BusinessApi.Policies;

public enum NonStringNullHandling { Skip = 0, AsToken = 1 }

public abstract record SoapUpdatePolicyBase
{
	// string null -> "" (unless column is in SkipEmptyStringColumns)
	public bool ClearNullStrings { get; init; } = false;

	// non-string null (DateTime/decimal/bool) handling (Skip is safest for DB)
	public NonStringNullHandling NonStringNulls { get; init; } = NonStringNullHandling.Skip;

	// used only if NonStringNulls == AsToken; backend must interpret as SQL NULL
	public string? NullToken { get; init; } = null;

	// columns for which empty strings must never be sent (e.g., "Mobile")
	public abstract IReadOnlySet<string> SkipEmptyStringColumns { get; }
}