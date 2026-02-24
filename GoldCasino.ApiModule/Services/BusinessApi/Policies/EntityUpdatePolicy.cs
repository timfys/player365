namespace GoldCasino.ApiModule.Services.BusinessApi.Policies;

public sealed record EntityUpdatePolicy : SoapUpdatePolicyBase
{
	private static readonly HashSet<string> _skip =
		new(StringComparer.OrdinalIgnoreCase) { "Mobile" };

	public override IReadOnlySet<string> SkipEmptyStringColumns => _skip;
}