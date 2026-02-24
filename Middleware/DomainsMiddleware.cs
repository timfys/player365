//using System;
//using System.Globalization;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
//using SmartWinners.Configuration;
//using SmartWinners.Controllers;
//using SmartWinners.Helpers;

//namespace SmartWinners.Middleware;

//public class DomainsMiddleware
//{
//    private readonly RequestDelegate _next;

//    public DomainsMiddleware(RequestDelegate next)
//    {
//        _next = next;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            if (!context.Request.Path.StartsWithSegments("/assets") &&
//                !context.Request.Path.StartsWithSegments("/images") &&
//                !context.Request.Path.StartsWithSegments("/MailingTemplates") &&
//                !context.Request.Path.StartsWithSegments("/slotmachinescript") &&
//                !context.Request.Path.StartsWithSegments("/umbraco") &&
//                !context.Request.Path.StartsWithSegments("/favicon.png") &&
//                !context.Request.Path.StartsWithSegments("/faviconIwin.png") &&
//                !context.Request.Path.StartsWithSegments("/favicon.ico") &&
//                !context.Request.Path.StartsWithSegments("/favicon.jpg") &&
//                !context.Request.Path.Value.Contains("gateaway", StringComparison.OrdinalIgnoreCase) &&
//                !context.Request.Path.Value.ToLower().Contains("webhook", StringComparison.OrdinalIgnoreCase))
//            {
//                if (context.Request.Method.Equals("GET") && !context.Request.Headers.ContainsKey("X-Fetch-Indicator"))
//                {
//                    var host = context.Request.Host.Value;

//                    //if (!host.Contains("www") && host.Equals("playerclub365.app", StringComparison.OrdinalIgnoreCase))
//                    //{
//                    //    context.Response.Redirect($"https://www.{host}{context.Request.Path}{context.Request.QueryString}");
//                    //    return;
//                    //}

//                    var cultureInfo = Thread.CurrentThread.CurrentCulture;
//                    var langIso = cultureInfo.TwoLetterISOLanguageName;
//                    langIso = langIso.Equals("en") ? "" : $"/{langIso}";

//                    var tempLangIso = GetUserLangIsoFromUrl(context);

//                    var heLangIso = "/he";
//                    var enLangIso = "";


//                    switch (tempLangIso)
//                    {
//                        /*case "":
//                        {
//                            if (context.Request.Host.Value.Contains("smartwinners.co.il") ||
//                                context.Request.Host.Value.Contains("iwin.co.il") || true)
//                            {
//                                cultureInfo = new CultureInfo(1037);
//                                enLangIso = "/en";
//                                heLangIso = "";
//                                WebStorageUtility.SetString(WebStorageUtility.LangIso, "he");
//                                WebStorageUtility.CurrentLangIso = "he";
//                            }
//                            else
//                            {
//                                cultureInfo = new CultureInfo(1033);
//                                WebStorageUtility.SetString(WebStorageUtility.LangIso, "en");
//                                WebStorageUtility.CurrentLangIso = "en";
//                            }

//                            break;
//                        }*/

//                        case /*"en"*/"":
//                        {
//                            cultureInfo = new CultureInfo(1033);
//                            WebStorageUtility.SetString(WebStorageUtility.LangIso, "en");
//                            WebStorageUtility.CurrentLangIso = "en";
//                            break;
//                        }
//                        case "he":
//                        {
//                            cultureInfo = new CultureInfo(1037);
//                            WebStorageUtility.SetString(WebStorageUtility.LangIso, "he");
//                            WebStorageUtility.CurrentLangIso = "he";
//                            break;
//                        }
//                        case "es":
//                        {
//                            cultureInfo = new CultureInfo(3082);
//                            WebStorageUtility.SetString(WebStorageUtility.LangIso, "es");
//                            WebStorageUtility.CurrentLangIso = "es";
//                            break;
//                        }
//                        case "fr":
//                        {
//                            cultureInfo = new CultureInfo(1036);
//                            WebStorageUtility.SetString(WebStorageUtility.LangIso, "fr");
//                            WebStorageUtility.CurrentLangIso = "fr";
//                            break;
//                        }
//                        case "ru":
//                        {
//                            cultureInfo = new CultureInfo(1049);
//                            WebStorageUtility.SetString(WebStorageUtility.LangIso, "ru");
//                            WebStorageUtility.CurrentLangIso = "ru";
//                            break;
//                        }
//                        case "uk":
//                        {
//                            cultureInfo = new CultureInfo(1058);
//                            WebStorageUtility.SetString(WebStorageUtility.LangIso, "uk");
//                            WebStorageUtility.CurrentLangIso = "uk";
//                            break;
//                        }
//                    }


//                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
//                    Thread.CurrentThread.CurrentCulture = cultureInfo;
//                    tempLangIso = string.IsNullOrEmpty(tempLangIso) ? "" : $"/{tempLangIso}";
//                    context.Items.Add("CultureInfo", cultureInfo);
//                    context.Items.Add("HeLangIso", heLangIso);
//                    context.Items.Add("EnLangIso", enLangIso);
//                    context.Items.Add("LangIsoMaster", tempLangIso);
//                }
//            }


//            await _next(context);
//        }
//        catch (Exception e)
//        {
//            ErrorsHandlerController.HandleMiddlewareError(e, context);
//        }
//        finally
//        {
//            WebStorageUtility.EndRequest();
//        }
//    }

//    public static string GetUserLangIsoFromUrl(HttpContext context)
//    {
//        var langIsoSplit = context.Request.Path.Value.Split("/");

//        if (context.Request.Path.Value.StartsWith("/") && context.Request.Path.Value.Length == 3)
//        {
//            return langIsoSplit[1];
//        }

//        if (langIsoSplit.Length > 2)
//        {
//            return langIsoSplit[1].Length == 2 ? langIsoSplit[1] : "";
//        }
//        else
//        {
//            return langIsoSplit[0];
//        }
//    }
//}