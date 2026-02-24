using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace SmartWinners.Controllers;

public class SWSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider) : SurfaceController(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
{
    protected void LoadForm<TModel>(TModel model)
        where TModel : class
    {
        ModelState.Clear();

        var formDic = Request.Form.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault());
        
        var json = JsonConvert.SerializeObject(formDic);

        JsonConvert.PopulateObject(json, model, new JsonSerializerSettings
        {
            Error = (obj, args) =>
            {
                ModelState.AddModelError(args.ErrorContext?.Path ?? "", "Data error");
                args.ErrorContext.Handled = true;
            }
        });
        
        TryValidateModel(model);
    }

    protected void ModelStateSetValid(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var entry = ModelState.GetValueOrDefault(name);
            if (entry != null)
            {
                entry.Errors.Clear();
                entry.ValidationState = ModelValidationState.Valid;
            }
        }
    }

    protected bool ModelStateIsValid(string key)
    {
        var state = ModelState.FirstOrDefault(x => x.Key == key).Value?.ValidationState;
        return state == null || state == ModelValidationState.Valid;
    }

    protected static bool IsGet(HttpContext context)
        => context?.Request?.Method?.ToLower() == "get";
}