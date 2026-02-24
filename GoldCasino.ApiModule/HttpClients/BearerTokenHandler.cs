using System.Net.Http.Headers;

namespace GoldCasino.ApiModule.HttpClients;

public sealed class BearerTokenHandler(ITokenStore store) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request, CancellationToken ct)
	{
		var token = store.Current;
		if (!string.IsNullOrWhiteSpace(token))
			request.Headers.Authorization =
					new AuthenticationHeaderValue("Bearer", token);

		return await base.SendAsync(request, ct);
	}
}