// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using SmartWinners.Helpers;
// using System;
// using System.IO;
// using System.Linq;
// using System.Threading.Tasks;
// using System.Xml.Linq;

// namespace SmartWinners.Middleware;

// public class IpMiddleware(RequestDelegate next, ILogger<IpMiddleware> logger)
// {
// 	public async Task InvokeAsync(HttpContext context)
// 	{

// 		/*if (context.Request.Host.Value.Equals("188.130.240.56") && !IdentityHelper.GetUserIp().Equals("188.130.240.56"))
// 		{
// 				context.Response.StatusCode = 403;
// 				return;
// 		}*/

// 		try
// 		{

// 			if (context.Request.Headers["X-Forwarded-Proto"].Equals("http") &&
// 					!context.Request.Host.Value.Equals("89.23.5.24"))
// 			{
// 				context.Response.Redirect(
// 						$"https://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}");
// 				return;
// 			}

// 			if (!context.Request.Path.Value.ToLower().Contains("/assets") &&
// 					!context.Request.Path.Value.ToLower().Contains("/images") &&
// 					!context.Request.Path.Value.ToLower().Contains("/mailingtemplates") &&
// 					!context.Request.Path.Value.ToLower().Contains("/slotmachinescript") &&
// 					!context.Request.Path.Value.ToLower().Contains("js") &&
// 					!context.Request.Path.Value.ToLower().Contains("css") &&
// 					!context.Request.Path.Value.ToLower().Contains("webhook"))
// 			{
// 				try
// 				{
// 					WebStorageUtility.EndRequest();
// 					if (CheckIfUserIsBlocked(IdentityHelper.GetUserIp(), context.Request.Path.Value) ||
// 							CheckForRestrictedUrlAccessAttempt(IdentityHelper.GetUserIp(), context.Request.Path.Value))
// 					{
// 						context.Response.StatusCode = 403;
// 						return;
// 					}
// 				}
// 				catch (Exception e)
// 				{

// 				}
// 			}


// 			await next.Invoke(context);
// 		}
// 		catch (Exception e)
// 		{
// 			logger.LogError(e, "Some error occurred");
// 		}
// 		finally
// 		{
// 			WebStorageUtility.EndRequest();
// 		}
// 	}

// 	static bool CheckForRestrictedUrlAccessAttempt(string ipAddress, string path)
// 	{
// 		var filePath = $"{EnvironmentHelper.Environment.WebRootPath}/IpAddressesList.xml";
// 		var doc = XDocument.Load(filePath);

// 		var root = doc.Root;

// 		var elements = root.Elements();

// 		var whiteListedIp = elements.First(x => x.Name.LocalName.Equals("whiteListedAddresses")).Elements()
// 				.FirstOrDefault(x => x.Value.Equals(ipAddress));

// 		var configElements =
// 				elements.First(x => x.Name.LocalName.Equals("listsConfig", StringComparison.OrdinalIgnoreCase)).Elements();

// 		var bannedUrls = configElements
// 				.First(x => x.Name.LocalName.Equals("restirectedUrls", StringComparison.OrdinalIgnoreCase))
// 				.Elements().ToList();

// 		var bannedUrl = bannedUrls.Any(x =>
// 				x.Attribute("isUrlPart").Value.Equals("true")
// 						? path.Contains(x.Value.Replace("\n", "").Trim())
// 						: path.Equals(x.Value.Replace("\n", "").Trim()));

// 		var whiteListedUrl = whiteListedIp?.Attribute("allowedUrls")?.Value;

// 		if (bannedUrl
// 				&& (whiteListedIp is null || !whiteListedUrl.Split(",")
// 						.Any(x => path.Contains(x, StringComparison.OrdinalIgnoreCase) || x.Contains("*"))))
// 		{
// 			var blackListedIps = elements.First(x => x.Name.LocalName.Equals("blackListedAddresses"));

// 			var blackListedIp = blackListedIps.Elements()
// 					.FirstOrDefault(x => x.Value.Equals(ipAddress));

// 			var isAlreadyBanned = blackListedIp is not null;
// 			blackListedIp ??= new XElement("ip");

// 			var banTimeElem =
// 					configElements.First(x => x.Name.LocalName.Equals("banTime", StringComparison.OrdinalIgnoreCase));

// 			var banForSpecificUrlElem = configElements.First(x =>
// 					x.Name.LocalName.Equals("banForSpecificUrl", StringComparison.OrdinalIgnoreCase));

// 			var blockedUntil = blackListedIp.Attribute("blockedUntil");
// 			var restrictedUrls = blackListedIp.Attribute("restrictedUrls");

// 			blockedUntil?.Remove();
// 			if (restrictedUrls is null)
// 			{
// 				restrictedUrls = new XAttribute("restrictedUrls", path);
// 			}
// 			else if (!restrictedUrls.Value.Contains(path, StringComparison.OrdinalIgnoreCase))
// 			{
// 				restrictedUrls.Remove();

// 				if (!restrictedUrls.Value.Contains(path))
// 					restrictedUrls.Value += $",{path}";
// 			}

// 			if (banForSpecificUrlElem.Value.Contains("false", StringComparison.OrdinalIgnoreCase))
// 			{
// 				restrictedUrls.Value = "*";
// 			}

// 			blackListedIp.Add(new XAttribute("blockedUntil",
// 					$"{DateTime.UtcNow + TimeSpan.FromHours(int.Parse(banTimeElem.Value)):yyyy-MM-dd}"));
// 			blackListedIp.Add(restrictedUrls);

// 			if (!isAlreadyBanned)
// 			{
// 				blackListedIp.Value = ipAddress;
// 				blackListedIps.Add(blackListedIp);
// 			}

// 			doc.Save(filePath);
// 			return true;
// 		}

// 		return false;
// 	}

// 	static bool CheckIfUserIsBlocked(string ipAddress, string path)
// 	{
// 		var filePath = $"{EnvironmentHelper.Environment.WebRootPath}/IpAddressesList.xml";

// 		var doc = XDocument.Load(filePath);
// 		var root = doc.Root;

// 		var blackListedIps = root?.Elements()
// 				.First(x => x.Name.LocalName.Equals("blackListedAddresses", StringComparison.OrdinalIgnoreCase)).Elements()
// 				.ToList();


// 		var bannedIp = blackListedIps?.FirstOrDefault(x => x.Value.Equals(ipAddress));

// 		if (bannedIp is null)
// 			return false;

// 		var bannedUrlsStr = bannedIp?.Attribute("restrictedUrls").Value;
// 		var bannedUrls = bannedIp?.Attribute("restrictedUrls").Value.Split(",");
// 		var banTimeStr = bannedIp?.Attribute("blockedUntil").Value;

// 		if (banTimeStr?.Equals("*") || !DateTime.TryParse(banTimeStr, out var banTime) || banTime > DateTime.UtcNow
// 				&& (bannedUrls?.Any(x => path.Contains(x, StringComparison.OrdinalIgnoreCase)) || bannedUrlsStr.Equals("*")))
// 		{
// 			var banForSpecificUrl = root.Elements().First(x => x.Name.LocalName.Equals("listsConfig")).Elements().First(
// 							x =>
// 									x.Name.LocalName.Equals("banForSpecificUrl", StringComparison.OrdinalIgnoreCase)).Value
// 					.Equals("true");

// 			if (!banForSpecificUrl && !bannedUrlsStr.Equals("*"))
// 			{
// 				bannedIp.Attribute("restrictedUrls").Value = "*";
// 				doc.Save(filePath);
// 			}

// 			return true;
// 		}

// 		if (DateTime.TryParse(banTimeStr, out banTime) && banTime < DateTime.UtcNow)
// 		{
// 			bannedIp.Remove();
// 			doc.Save(filePath);
// 		}

// 		return false;
// 	}

// 	bool CheckIfIpFileIsUsed()
// 	{
// 		try
// 		{
//       using var f = new FileStream($"{EnvironmentHelper.Environment.WebRootPath}/IpAddressesList.xml",
//                  FileMode.Open);
//       return false;
//     }
// 		catch (Exception e)
// 		{
// 			return true;
// 		}

// 	}
// }