using GoldCasino.ApiModule.Models;

namespace GoldCasino.ApiModule.Services.SmartWinnersApi;

public interface ISmartWinnersApiService
{
	Task<Result<SignWithPhoneResult, Error>> SignInWithPhoneAsync(SignWithPhone model);
	Task<Result<VouchersGetResult, Error>> VouchersGetAsync(VouchersGet model, UserApiAccess? accessData = null);
	Task<Result<string, Error>> EntityBalanceCalcAsync(UserApiAccess accessData);
}
