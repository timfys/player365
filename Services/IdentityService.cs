using SmartWinners.Models.BusinessAPI.Entity;
using System.Threading.Tasks;

namespace SmartWinners.Services;

	public class IdentityService
	{
		public async Task<EntityVerifyContactResponse> VerifyPhone(int id)
		{
			var businessApiService = new BusinessApiService();
			return await businessApiService.EntityVerify(new()
				{ EntityId = id, VerificationType = VerificationType.Mobile, VerificationCode = "" });
		}

		public async Task<EntityVerifyContactResponse> VerifyPhone(int id, string code)
		{
			var businessApiService = new BusinessApiService();
			return await businessApiService.EntityVerify(new()
				{ EntityId = id, VerificationType = VerificationType.Mobile, VerificationCode = code });
		}

		public async Task<EntityVerifyContactResponse> VerifyEmail(int id)
		{
			var businessApiService = new BusinessApiService();
			return await businessApiService.EntityVerify(new()
				{ EntityId = id, VerificationType = VerificationType.Email, VerificationCode = "" });
		}

		public async Task<EntityVerifyContactResponse> VerifyEmail(int id, string code)
		{
			var businessApiService = new BusinessApiService();
			return await businessApiService.EntityVerify(new()
				{ EntityId = id, VerificationType = VerificationType.Email, VerificationCode = code });
		}
	}
