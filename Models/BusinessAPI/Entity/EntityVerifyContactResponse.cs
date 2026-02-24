

namespace SmartWinners.Models.BusinessAPI.Entity;

	public class EntityVerifyContactResponse: ApiResponse
	{
		public string EntityId { get; set; }

		public const int ResultCodeVerificationTemplateNotConfigured = -12;
		public const int ResultCodeVerificationTemplateForBusinessNotConfigured = -13;
		public const int ResultCodeVerificationCodeIncorrect = -14;
		public const int ResultCodeVerificationCodeAlreadySent = 16;

		public bool IsVerificationTemplateNotConfigured => ResultCode == ResultCodeVerificationTemplateNotConfigured;
		public bool IsVerificationTemplateForBusinessNotConfigured => ResultCode == ResultCodeVerificationTemplateForBusinessNotConfigured;
		public bool IsVerificationCodeIncorrect => ResultCode == ResultCodeVerificationCodeIncorrect;
		public bool IsVerificationCodeAlreadySent => ResultCode == ResultCodeVerificationCodeAlreadySent;
	}