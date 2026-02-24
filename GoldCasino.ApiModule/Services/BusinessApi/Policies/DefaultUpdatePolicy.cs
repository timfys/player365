namespace GoldCasino.ApiModule.Services.BusinessApi.Policies;

public sealed record DefaultUpdatePolicy : SoapUpdatePolicyBase
{
	private static readonly HashSet<string> _skip = new(StringComparer.OrdinalIgnoreCase);
	public override IReadOnlySet<string> SkipEmptyStringColumns => _skip;
}