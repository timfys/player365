using System;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public sealed class OutgoingAdd
{
	// Email metadata
	public string From { get; set; } = string.Empty;
	public string Destination { get; set; } = string.Empty;
	public int Priority { get; set; } = 0;
	public int MessageId { get; set; } = 0;
	public int OrderDocumentId { get; set; } = 0;
	public string ScheduleTo { get; set; } = string.Empty;
	public int[] EntityIds { get; set; } = [4];

	// Payload values
	public int EntityId { get; set; }
	public string EntityName { get; set; } = string.Empty;
	public string EntityMobile { get; set; } = string.Empty;
	public string GameName { get; set; } = string.Empty;
	public string GameUrl { get; set; } = string.Empty;
	public string GameId { get; set; } = string.Empty;
	public DateTimeOffset? Timestamp { get; set; }
}
