using GoldCasino.ApiModule.Mapping;

namespace GoldCasino.ApiModule.Dtos.Files;

public class EntityFileMinDto
{
	[EntityField("fileid")] public int FileId { get; set; }
	[EntityField("Filename")] public string Filename { get; set; } = string.Empty;
	[EntityField("FileData")] public byte[] FileData { get; set; } = [];
}

public class EntityFileDto
{
	[EntityField("fileid")] public int FileId { get; set; }
	[EntityField("EntityId")] public int EntityId { get; set; }
	[EntityField("Filename")] public string Filename { get; set; } = string.Empty;
	[EntityField("FileData")] public byte[] FileData { get; set; } = [];
	[EntityField("width")] public int? Width { get; set; }
	[EntityField("Height")] public int? Height { get; set; }
	[EntityField("Top")] public int? Top { get; set; }
	[EntityField("Aleft")] public int? Aleft { get; set; }
	[EntityField("Label")] public string? Label { get; set; }
	[EntityField("date")] public DateTime? Date { get; set; }
	[EntityField("Type")] public string? Type { get; set; }
	[EntityField("UserId")] public int? UserId { get; set; }
	[EntityField("isESigned")] public bool? IsESigned { get; set; }
	[EntityField("documentId")] public int? DocumentId { get; set; }
	[EntityField("Order_DocumentId")] public int? OrderDocumentId { get; set; }
	[EntityField("sync_modified_date")] public DateTime? SyncModifiedDate { get; set; }
}
