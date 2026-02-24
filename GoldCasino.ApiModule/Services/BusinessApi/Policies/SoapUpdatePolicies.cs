namespace GoldCasino.ApiModule.Services.BusinessApi.Policies;

public static class SoapUpdatePolicies
{
	private sealed record DefaultPolicy : SoapUpdatePolicyBase
	{
		private static readonly IReadOnlySet<string> _skip =
			new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		public override IReadOnlySet<string> SkipEmptyStringColumns => _skip;
	}

	private sealed record EntityPolicy : SoapUpdatePolicyBase
	{
		private static readonly IReadOnlySet<string> _skip =
		new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Mobile" };
		public override IReadOnlySet<string> SkipEmptyStringColumns => _skip;
	}

	public static readonly SoapUpdatePolicyBase Default = new DefaultPolicy();
	public static readonly SoapUpdatePolicyBase Entity = new EntityPolicy();
}