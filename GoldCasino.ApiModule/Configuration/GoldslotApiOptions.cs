using System.ComponentModel.DataAnnotations;

namespace GoldCasino.ApiModule.Configuration;

public class GoldSlotApiOptions
{
	[Url]
	public required string BaseUrl { get; set; } = default!;

	public required string AccessToken { get; set; }
}
