using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Findx.AspNetCore.Mvc;

/// <summary>
///     视图读取
/// </summary>
public class RazorViewRender : IRazorViewRender
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IRazorViewEngine _viewEngine;

    /// <summary>
    ///     Ctor
    /// </summary>
    /// <param name="viewEngine"></param>
    /// <param name="tempDataProvider"></param>
    /// <param name="serviceProvider"></param>
    public RazorViewRender(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     读取视图内容
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="viewName"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName);

        using (var output = new StringWriter())
        {
            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    { Model = model },
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                output,
                new HtmlHelperOptions());

            await view.RenderAsync(viewContext);

            return output.ToString();
        }
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _viewEngine.GetView(null, viewName, true);
        if (getViewResult.Success) return getViewResult.View;

        var findViewResult = _viewEngine.FindView(actionContext, viewName, true);
        if (findViewResult.Success) return findViewResult.View;

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(
                searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}