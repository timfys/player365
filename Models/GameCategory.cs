using System.Text.Json.Serialization;

namespace SmartWinners.Models;

public class GameCategory
{
	[JsonPropertyName("categoryId")]
	public int CategoryID { get; set; }
	[JsonPropertyName("category_name")]
	public string CategoryName { get; set; }
	[JsonPropertyName("is_dynamic")]
	public int IsDynamic { get; set; }
	[JsonPropertyName("enabled")]
	public int Enabled { get; set; }
	public int? ExecuteTime { get; set; }
}