namespace SmartWinners.Models.BusinessAPI.Entity;

	public class EntityVerifyContact
	{
		public int EntityId { get; set; }
		public VerificationType VerificationType { get; set; }
		public string VerificationCode { get; set; }
	}

	public enum VerificationType { Mobile, Email }