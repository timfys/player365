using GoldCasino.ApiModule.Extensions;
using GoldCasino.ApiModule.Models;
using GoldCasino.ApiModule.Services.BusinessApi;
using GoldCasino.ApiModule.Services.BusinessApi.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoldCasino.ApiModule.Helpers;

public static class TrafficLogHelper
{
    private const string VisitedBeforeSignUpCookie = "45igjyf5496yh5486y0j5846jf5465"; // matches WebStorageUtility.VisitedBeforeSignUp

    public static async Task LogTrafficAsync(HttpContext? context, IBusinessApiService? businessApiService)
    {
        if (context is null || businessApiService is null)
            return;

        try
        {
            var claimsAccess = context.User.ToUserApiAccess();
            if (claimsAccess is null)
                return;

            var payload = BuildLogModel(context, claimsAccess.EntityId);
            if (payload is null)
                return;

            await businessApiService.EntityLogTrafficAsync(payload, claimsAccess);
        }
        catch
        {
            return;
        }
    }

    public static async Task LogBeforeLoginVisitedPagesAsync(HttpContext? context, IBusinessApiService? businessApiService, int entityId)
    {
        if (context is null || businessApiService is null || entityId <= 0)
            return;

        try
        {
            var visitedPages = GetBeforeSignupVisitedPages(context);
            if (visitedPages.Count == 0)
                return;

            var userAgent = context.Request?.Headers.UserAgent.ToString() ?? string.Empty;
            var deviceKind = ResolveDeviceKind(userAgent);
            var ipAddress = ResolveIp(context) ?? string.Empty;

            var tasks = visitedPages
                .OrderBy(static page => page.UTCDateTime)
                .Select(page =>
                {
                    var fields = BuildExtraFields(page);

                    var payload = new EntityLogTraffic
                    {
                        EntityId = entityId,
                        DeviceKind = deviceKind,
                        SystemInfo = userAgent,
                        IpAddress = ipAddress,
                        Url = page.PagePath,
                        Fields = fields
                    };

                    return businessApiService.EntityLogTrafficAsync(payload);
                });

            await Task.WhenAll(tasks);
        }
        catch
        {
            return;
        }
    }

    public static void ClearBeforeSignupVisitedPages(HttpContext? context)
    {
        context?.Response.Cookies.Delete(VisitedBeforeSignUpCookie);
    }

    private static EntityLogTraffic? BuildLogModel(HttpContext context, int entityId)
    {
        var request = context.Request;
        if (request is null)
            return null;

        var userAgent = request.Headers.UserAgent.ToString();
        var fields = BuildExtraFields(request);

        return new EntityLogTraffic
        {
            EntityId = entityId,
            DeviceKind = ResolveDeviceKind(userAgent),
            SystemInfo = userAgent,
            IpAddress = ResolveIp(context),
            Url = $"{request.Host}{request.Path}{request.QueryString}",
            Fields = fields
        };
    }

    private static Dictionary<string, string>? BuildExtraFields(HttpRequest request)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var referer = request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer))
            fields["Referer"] = referer;

        var language = request.Headers.AcceptLanguage.ToString();
        if (!string.IsNullOrWhiteSpace(language))
            fields["AcceptLanguage"] = language;

        return fields.Count > 0 ? fields : null;
    }

    private static Dictionary<string, string>? BuildExtraFields(BeforeLoginVisitedPageInfo page)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["VisitedAtUtc"] = page.UTCDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        };

        if (!string.IsNullOrWhiteSpace(page.Referer))
            fields["Referer"] = page.Referer!;

        return fields.Count > 0 ? fields : null;
    }

    private static List<BeforeLoginVisitedPageInfo> GetBeforeSignupVisitedPages(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(VisitedBeforeSignUpCookie, out var value) || string.IsNullOrWhiteSpace(value))
            return [];

        try
        {
            var decoded = WebUtility.UrlDecode(value) ?? string.Empty;
            decoded = decoded.Replace(" ", "+");
            var bytes = Convert.FromBase64String(decoded);

            using var inputStream = new MemoryStream(bytes);
            using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            using var outputStream = new MemoryStream();
            gzipStream.CopyTo(outputStream);
            var json = Encoding.UTF8.GetString(outputStream.ToArray());

            return JsonSerializer.Deserialize<List<BeforeLoginVisitedPageInfo>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static DeviceKind ResolveDeviceKind(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return DeviceKind.Web;

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("android"))
            return DeviceKind.Android;

        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ios"))
            return DeviceKind.Ios;

        if (ua.Contains("windows phone"))
            return DeviceKind.WindowsPhone;

        return DeviceKind.Web;
    }

    private static string? ResolveIp(HttpContext context)
    {
        var cfIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(cfIp))
            return cfIp;

        var xff = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(xff))
        {
            var firstIp = xff.Split(',').Select(static x => x.Trim()).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstIp))
                return firstIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
