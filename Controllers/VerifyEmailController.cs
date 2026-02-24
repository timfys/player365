using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SmartWinners.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using BusinessApi;
using static SmartWinners.Helpers.IdentityHelper;

namespace SmartWinners.Controllers;

public class VerifyEmailController(
    IUmbracoContextAccessor umbracoContextAccessor,
    IUmbracoDatabaseFactory databaseFactory,
    ServiceContext services,
    AppCaches appCaches,
    IProfilingLogger profilingLogger,
    IPublishedUrlProvider publishedUrlProvider) : SWSurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
    // [HttpPost]
    // [Route("VerifyEmail/SendAgain")]
    // [IgnoreAntiforgeryToken]
    // public IActionResult SendAgain()
    // {
    //     try
    //     {
    //         var user = WebStorageUtility.GetSignedUser(HttpContext);

    //         if (string.IsNullOrEmpty((string)WebStorageUtility.GetEntityField(user.EntityId, "CustomField68")))
    //         {
    //             UpdateEntity(new Dictionary<string, string>{{"CustomField68", HttpContext.Request.Host.Value}}, user.EntityId, out var resp);
    //         }
            
    //         var queryEntity = HttpContext.Request.Query["eID"].FirstOrDefault();
    //         var config = EnvironmentHelper.BusinessApiConfiguration;
    //         var api = new IdentityHelper();

    //         var request = new Entity_VerifyContactInfoRequest
    //         {
    //             ol_EntityID = config.ol_EntityId,
    //             ol_Username = config.ol_UserName,
    //             ol_Password = config.ol_Password,
    //             businessId = config.BusinessId,
    //             entityID = user.EntityId,
    //             VerifyType = 1,
    //             VerificationCode = "",
    //         };

    //         var businessClient = EnvironmentHelper.BusinessApiConfiguration.InitClient();

    //         var apiResponse = businessClient.Entity_VerifyContactInfo(request);

    //         var response = JsonConvert.DeserializeObject<GeneralApiResponse>(apiResponse.@return);

    //         if (response.ResultCode >= 0)
    //         {
    //             return Ok(response);
    //         }

    //         return BadRequest(response);
    //     }
    //     catch (Exception e)
    //     {
    //         return BadRequest();
    //     }
    // }
}