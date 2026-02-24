namespace SmartWinners.Helpers;

using Microsoft.AspNetCore.Http;

public static class UmbracoUtility
{
    public static string GetReturnUrl(HttpContext context, string defaultValue = "./")
    {
        context.Request.Query.TryGetValue("r", out var path);
        return string.IsNullOrEmpty(path) ? defaultValue : path;
    }
}