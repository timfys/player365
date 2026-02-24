using System.ComponentModel.DataAnnotations;

namespace SlotsTest.Server.Configuration;

public class GoldslotApiOptions
{
	[Url]
	public required string BaseUrl { get; set; } = default!;

	public required string AccessToken { get; set; }
}
