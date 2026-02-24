using System.Text.Json.Serialization;
using GoldCasino.ApiModule.Convertors;

namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public sealed class EntityFilesGet
{
	public int? LimitFrom { get; set; }
	public int? LimitCount { get; set; }
	public int? BusinessId { get; set; }
	public string[]? Fields { get; set; }
	public Dictionary<string, string>? Filter { get; set; }
}

public sealed class EntityFilesGetResult
{
	public List<EntityFile> Files { get; init; } = [];
}

public sealed class EntityFile
{
	[JsonPropertyName("fileid")] public int FileId { get; set; }
	[JsonPropertyName("Filename")] public string Filename { get; set; } = string.Empty;
	
  [JsonConverter(typeof(ByteArrayConverter))] 
  [JsonPropertyName("FileData")] public byte[] FileData { get; set; } = [];
}
