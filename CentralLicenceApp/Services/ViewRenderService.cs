using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace CentralLicenceApp.Services
{
    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine    _viewEngine;
        private readonly ITempDataProvider   _tempDataProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider    _serviceProvider;

        public ViewRenderService(
            IRazorViewEngine     viewEngine,
            ITempDataProvider    tempDataProvider,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider     serviceProvider)
        {
            _viewEngine           = viewEngine;
            _tempDataProvider     = tempDataProvider;
            _httpContextAccessor  = httpContextAccessor;
            _serviceProvider      = serviceProvider;
        }

        public async Task<string> RenderToStringAsync(string viewPath, object model)
        {
            // Re-use the current HTTP context so Url.Content / scheme / host resolve correctly.
            // Fall back to a synthetic context when called outside an HTTP pipeline (e.g. background tasks).
            var httpContext = _httpContextAccessor.HttpContext
                ?? new DefaultHttpContext { RequestServices = _serviceProvider };

            var routeData    = httpContext.GetRouteData() ?? new RouteData();
            var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());

            // Build the full ~/Views/... path so GetView can locate it without a controller context.
            var fullViewPath = viewPath.StartsWith("~/") || viewPath.StartsWith("/")
                ? viewPath
                : $"~/Views/{viewPath}.cshtml";

            var viewResult = _viewEngine.GetView(null, fullViewPath, true);
            if (!viewResult.Success)
                viewResult = _viewEngine.FindView(actionContext, viewPath, false);

            if (!viewResult.Success)
                throw new InvalidOperationException($"Razor view '{viewPath}' not found. Searched: {string.Join(", ", viewResult.SearchedLocations ?? Array.Empty<string>())}");

            using var sw = new StringWriter();

            var viewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            { Model = model };

            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewData,
                tempData,
                sw,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
