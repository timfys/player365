using GoldCasino.ApiModule.Common.Patching;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi.Models;

namespace GoldCasino.ApiModule.Services.BusinessApi;

public interface IBusinessApiService
{
	Task<Result<EntityFindResult, Error>> EntityFindAsync(EntityFind model, UserApiAccess? accessData = null);
	Task<Result<bool, Error>> EntityUpdateAsync(Action<Patch<Entity>> configure, ImagePatch? images, UserApiAccess? accessData = null);
	Task<Result<bool, Error>> EntityUpdateAsync<TDto>(int entityId, TDto dto, UserApiAccess? accessData = null);
	Task<Result<bool, Error>> EntityUpdateAsync<TDto, TImgDto>(int entityId, TDto fieldsDto, TImgDto? imagesDto, UserApiAccess? accessData = null);
	Task<Result<bool, Error>> EntityVerifyContactInfoAsync(EntityVerifyContactInfo model, UserApiAccess? accessData = null);
	Task<Result<OlLoginResult, Error>> OlLoginAsync(OlLogin model);
	Task<Result<EntityAddResult, Error>> EntityAddAsync(EntityAdd model, UserApiAccess? accessData = null);
	Task<Result<EntityFilesGetResult, Error>> EntityFilesGetAsync(EntityFilesGet model, UserApiAccess? accessData = null);
	Task<Result<EntityFilesUpdateResponse, Error>> EntityFilesUpdateAsync(EntityFilesUpdate model, UserApiAccess? accessData = null);
	Task<Result<bool, Error>> EntityLogTrafficAsync(EntityLogTraffic model, UserApiAccess? accessData = null);
	Task<Result<string, Error>> OutgoingAddAsync(OutgoingAdd model, UserApiAccess? accessData = null);
	Task<Result<GeneralDecrypt, Error>> GeneralDecrypt(string lid);
	Task<Result<bool, Error>> EntityForgotPasswordAsync(EntityForgotPassword model);
	Task<Result<EntityBonusesGetResult, Error>> EntityBonusesGetAsync(EntityBonusesGet model, UserApiAccess? accessData = null);
}
