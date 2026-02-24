using GoldCasino.ApiModule.Auth;
using GoldCasino.ApiModule.Helpers;

namespace GoldCasino.ApiModule.Services;

public interface IAuthCookieService
{
    void Set(Credential credential);
    void Set(string encryptedToken);
    void Delete();
}

public class AuthCookieService(IHttpContextAccessor httpContextAccessor, CookieEncryptionHelper cookieEncryption, ILogger<AuthCookieService> logger) : IAuthCookieService
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;
    private readonly CookieEncryptionHelper _crypto = cookieEncryption;
    private readonly ILogger<AuthCookieService> _logger = logger;

    public void Set(Credential credential)
    {
        var encrypted = _crypto.Encrypt(credential);
        Set(encrypted);
    }

    public void Set(string encryptedToken)
    {
        var ctx = _http.HttpContext;
        if (ctx is null)
        {
            _logger.LogWarning("No HttpContext available when attempting to set auth cookie");
            return;
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = ctx.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/"
        };

        ctx.Response.Cookies.Append(AuthConstants.CookieName, encryptedToken, cookieOptions);
    }

    public void Delete()
    {
        var ctx = _http.HttpContext;
        if (ctx is null)
        {
            _logger.LogWarning("No HttpContext available when attempting to delete auth cookie");
            return;
        }

        ctx.Response.Cookies.Delete(AuthConstants.CookieName, new CookieOptions { Path = "/" });
    }
}
