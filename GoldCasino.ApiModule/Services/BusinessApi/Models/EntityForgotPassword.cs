namespace GoldCasino.ApiModule.Services.BusinessApi.Models;

public class EntityForgotPassword
{
	public RemindKind RemindKind { get; set; }
	public string? NewPassword { get; set; }
	public string? Language { get; set; }
	public string? TokenCode { get; set; }
	public string Domain { get; set; }
	public int InboxId { get; set; }
	public required string Username { get; set; }
}

public enum RemindKind
{
	Mail = 0,
	SMS = 1,
	Whatsapp = 3,
}