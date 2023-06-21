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
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="viewEngine"></param>
    /// <param name="tempDataProvider"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="serviceProvider"></param>
    public RazorViewRender(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _httpContextAccessor = httpContextAccessor;
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
        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            viewData,
            new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );
        await view.RenderAsync(viewContext);
        return output.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        return _httpContextAccessor.HttpContext != null ? new ActionContext(_httpContextAccessor.HttpContext, _httpContextAccessor.HttpContext.GetRouteData(), new ActionDescriptor()) 
            : new ActionContext(new DefaultHttpContext { RequestServices = _serviceProvider }, new RouteData(), new ActionDescriptor());
    }
}