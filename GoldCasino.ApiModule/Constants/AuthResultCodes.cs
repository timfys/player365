namespace GoldCasino.ApiModule.Constants;

public static class AuthResultCodes
{
    public const string Success = "SUCCESS";
    public const string AuthFailed = "AUTH_FAILED";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
    public const string LogoutSuccess = "LOGOUT_SUCCESS";

    public const string PhoneAlreadyRegistered = "PHONE_ALREADY_REGISTERED";
    public const string Cooldown = "COOLDOWN";
    public const string InvalidVerificationCode = "INVALID_VERIFICATION_CODE";
    public const string MobileNotFound = "MOBILE_NOT_FOUND";
    public const string MessageNotFound = "MESSAGE_NOT_FOUND";
    public const string InvalidPhoneNumber = "INVALID_PHONE_NUMBER";

    public const string UserNotFound = "USER_NOT_FOUND";

    public const string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
    public const string InvalidEmail = "INVALID_EMAIL";
}
