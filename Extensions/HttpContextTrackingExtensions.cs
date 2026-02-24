using Microsoft.AspNetCore.Http;
using SmartWinners.Helpers;
using SmartWinners.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartWinners.Extensions;

public static class HttpContextTrackingExtensions
{
    private const int MaxTrackedPages = 50;

    public static List<BeforeLoginVisitedPageInfo> GetBeforeSignupVisitedPages(this HttpContext context)
    {
        if (context is null)
            return [];

        try
        {
            if (WebStorageUtility.TryGetString(WebStorageUtility.VisitedBeforeSignUp, out var pagesStr)
                && !string.IsNullOrWhiteSpace(pagesStr))
            {
                return CompressUtility.UrlDeCompressObject<List<BeforeLoginVisitedPageInfo>>(pagesStr) ?? [];
            }
        }
        catch
        {
            return [];
        }

        return [];
    }

    public static void SetBeforeSignUpVisitedPages(this HttpContext context, string? path)
    {
        if (context is null || string.IsNullOrWhiteSpace(path))
            return;

        if (context.User?.Identity?.IsAuthenticated ?? false)
            return;

        try
        {
            var visitedPages = context.GetBeforeSignupVisitedPages();

            // On the very first page visit, capture the external referrer into a dedicated cookie
            if (visitedPages.Count == 0)
            {
                var referer = GetReferer(context);
                if (!string.IsNullOrWhiteSpace(referer) && IsExternalReferer(context, referer))
                {
                    WebStorageUtility.SetString(
                        WebStorageUtility.ExternalReferer,
                        referer,
                        DateTime.UtcNow.AddDays(30));
                }
            }

            var entry = new BeforeLoginVisitedPageInfo
            {
                PagePath = path,
                UTCDateTime = DateTime.UtcNow,
                Referer = visitedPages.Count == 0 ? GetReferer(context) : null
            };

            visitedPages.Add(entry);

            if (visitedPages.Count > MaxTrackedPages)
                visitedPages = visitedPages.Skip(visitedPages.Count - MaxTrackedPages).ToList();

            var compressed = CompressUtility.UrlCompressObject(visitedPages);
            WebStorageUtility.SetString(WebStorageUtility.VisitedBeforeSignUp, compressed, WebStorageUtility.LifetimeCookieDate);
        }
        catch
        {
            WebStorageUtility.RemoveValue(WebStorageUtility.VisitedBeforeSignUp);
        }
    }

    public static void ClearBeforeSignupVisitedPages(this HttpContext context)
    {
        WebStorageUtility.RemoveValue(WebStorageUtility.VisitedBeforeSignUp);
    }

    private static string? GetReferer(HttpContext context)
    {
        var referer = context.Request?.Headers["Referer"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(referer) ? null : referer;
    }

    private static bool IsExternalReferer(HttpContext context, string referer)
    {
        if (!Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            return false;

        var host = context.Request.Host.Host;
        return !refererUri.Host.Equals(host, StringComparison.OrdinalIgnoreCase);
    }
}
