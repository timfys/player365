using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SmartWinners.Helpers;

public class Extensions
{
    public static async Task<string> RenderViewToStringAsync<TModel>(ControllerContext controllerContext, string viewName,
        TModel model, IRazorViewEngine razorViewEngine, ITempDataProvider tempData)
    {
        var viewResult = razorViewEngine.GetView(null, viewName,  false);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"View '{viewName}' not found.");
        }

        var view = viewResult.View;

        using var sw = new StringWriter();
        var viewContext = new ViewContext(
            controllerContext,
            view,
            new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: controllerContext.ModelState)
            {
                Model = model
            },
            new TempDataDictionary(controllerContext.HttpContext, tempData),
            sw,
            new HtmlHelperOptions()
        );

        await view.RenderAsync(viewContext);
        return sw.ToString();
    }
}