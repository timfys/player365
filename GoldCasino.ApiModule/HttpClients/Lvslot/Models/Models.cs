using System.Text.Json.Serialization;

namespace GoldCasino.ApiModule.HttpClients.Lvslot.Models;
public class OpenGameRequest
{
	[JsonPropertyName("cmd")] public string Cmd { get; set; } = "openGame";
	[JsonPropertyName("hall")] public string Hall { get; set; } = "";
	[JsonPropertyName("domain")] public string Domain { get; set; } = "";
	[JsonPropertyName("exitUrl")] public string ExitUrl { get; set; } = "";
	[JsonPropertyName("language")] public string Language { get; set; } = "en";
	[JsonPropertyName("key")] public string Key { get; set; } = "";
	[JsonPropertyName("login")] public string Login { get; set; } = "";
	[JsonPropertyName("gameId")] public string GameId { get; set; } = "";
	[JsonPropertyName("cdnUrl")] public string CdnUrl { get; set; } = "";
	[JsonPropertyName("demo")] public string Demo { get; set; } = "0";
}

public class OpenGameResponse
{
	[JsonPropertyName("status")] public string Status { get; set; } = "";	
	[JsonPropertyName("error")] public string? Error { get; set; }
	[JsonPropertyName("content")] public OpenGameContent? Content { get; set; }
}

public class OpenGameContent
{
	[JsonPropertyName("game")] public GameData? Game { get; set; }
	[JsonPropertyName("gameRes")] public GameRes? GameRes { get; set; }
}

public class GameData
{
	[JsonPropertyName("url")] public string Url { get; set; } = "";
	[JsonPropertyName("iframe")] public int Iframe { get; set; }
	[JsonPropertyName("sessionId")] public string? SessionId { get; set; }
	[JsonPropertyName("width")] public string? Width { get; set; }
	[JsonPropertyName("vertical")] public string? Vertical { get; set; }
	[JsonPropertyName("withoutFrame")] public string? WithoutFrame { get; set; }
	[JsonPropertyName("rewriterule")] public string? RewriteRule { get; set; }
	[JsonPropertyName("localhost")] public string? Localhost { get; set; }
	[JsonPropertyName("exitButton_mobile")] public string? ExitButtonMobile { get; set; }
	[JsonPropertyName("exitButton")] public string? ExitButton { get; set; }
	[JsonPropertyName("disableReload")] public string? DisableReload { get; set; }
	[JsonPropertyName("wager")] public string? Wager { get; set; }
	[JsonPropertyName("bonus")] public string? Bonus { get; set; }
}

public class GameRes
{
	[JsonPropertyName("sessionId")] public string? SessionId { get; set; }
}