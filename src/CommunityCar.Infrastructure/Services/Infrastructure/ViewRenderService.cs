using CommunityCar.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.Infrastructure;

public class ViewRenderService : IViewRenderService
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ViewRenderService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ViewRenderService(
        IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        ILogger<ViewRenderService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> RenderViewToStringAsync(string viewName, object model)
    {
        return await RenderViewToStringAsync(viewName, model, null);
    }

    public async Task<string> RenderViewToStringAsync(string viewName, object model, ViewDataDictionary? viewData)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? CreateHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        using var sw = new StringWriter();

        try
        {
            var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
            }

            var viewDictionary = viewData ?? new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering view {ViewName}", viewName);
            throw;
        }
    }

    public async Task<string> RenderPartialViewToStringAsync(string partialViewName, object model)
    {
        var httpContext = _httpContextAccessor.HttpContext ?? CreateHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        using var sw = new StringWriter();

        try
        {
            var viewResult = _razorViewEngine.FindView(actionContext, partialViewName, true);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{partialViewName} does not match any available partial view");
            }

            var viewDictionary = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering partial view {PartialViewName}", partialViewName);
            throw;
        }
    }

    public async Task<string> RenderTemplateAsync(string templateName, object model, Dictionary<string, object>? additionalData = null)
    {
        // For email templates or other template rendering
        var templateData = new Dictionary<string, object>
        {
            ["Model"] = model,
            ["CurrentDate"] = DateTime.UtcNow,
            ["BaseUrl"] = GetBaseUrl()
        };

        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
            {
                templateData[kvp.Key] = kvp.Value;
            }
        }

        return await RenderViewToStringAsync(templateName, templateData);
    }

    public async Task<string> RenderEmailTemplateAsync(string templateName, object model, string recipientEmail, string subject = null)
    {
        var emailData = new
        {
            Model = model,
            RecipientEmail = recipientEmail,
            Subject = subject ?? "Notification",
            SentAt = DateTime.UtcNow,
            BaseUrl = GetBaseUrl()
        };

        return await RenderViewToStringAsync($"Emails/{templateName}", emailData);
    }

    public async Task<string> RenderNotificationTemplateAsync(string templateName, object model, string userId)
    {
        var notificationData = new
        {
            Model = model,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            BaseUrl = GetBaseUrl()
        };

        return await RenderViewToStringAsync($"Notifications/{templateName}", notificationData);
    }

    public async Task<RenderedViewResult> RenderViewWithMetadataAsync(string viewName, object model)
    {
        var html = await RenderViewToStringAsync(viewName, model);

        return new RenderedViewResult
        {
            Html = html,
            ViewName = viewName,
            ModelType = model?.GetType()?.Name ?? "Anonymous",
            RenderedAt = DateTime.UtcNow,
            SizeInBytes = System.Text.Encoding.UTF8.GetByteCount(html),
            WordCount = CountWords(html),
            HasImages = html.Contains("<img"),
            HasLinks = html.Contains("<a href"),
            HasScripts = html.Contains("<script")
        };
    }

    public async Task<List<RenderedViewResult>> RenderMultipleViewsAsync(Dictionary<string, object> viewModels)
    {
        var results = new List<RenderedViewResult>();

        foreach (var kvp in viewModels)
        {
            try
            {
                var result = await RenderViewWithMetadataAsync(kvp.Key, kvp.Value);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render view {ViewName}", kvp.Key);
                results.Add(new RenderedViewResult
                {
                    ViewName = kvp.Key,
                    Error = ex.Message,
                    RenderedAt = DateTime.UtcNow
                });
            }
        }

        return results;
    }

    private HttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };

        // Set basic request properties
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = "/";

        return httpContext;
    }

    private string GetBaseUrl()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var request = httpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        // Fallback for when no HttpContext is available
        return "https://localhost:5001";
    }

    private int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        // Remove HTML tags and count words
        var cleanText = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", "");
        return cleanText.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

public interface IViewRenderService
{
    Task<string> RenderViewToStringAsync(string viewName, object model);
    Task<string> RenderViewToStringAsync(string viewName, object model, ViewDataDictionary? viewData);
    Task<string> RenderPartialViewToStringAsync(string partialViewName, object model);
    Task<string> RenderTemplateAsync(string templateName, object model, Dictionary<string, object>? additionalData = null);
    Task<string> RenderEmailTemplateAsync(string templateName, object model, string recipientEmail, string subject = null);
    Task<string> RenderNotificationTemplateAsync(string templateName, object model, string userId);
    Task<RenderedViewResult> RenderViewWithMetadataAsync(string viewName, object model);
    Task<List<RenderedViewResult>> RenderMultipleViewsAsync(Dictionary<string, object> viewModels);
}

public class RenderedViewResult
{
    public string Html { get; set; } = string.Empty;
    public string ViewName { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public DateTime RenderedAt { get; set; }
    public int SizeInBytes { get; set; }
    public int WordCount { get; set; }
    public bool HasImages { get; set; }
    public bool HasLinks { get; set; }
    public bool HasScripts { get; set; }
    public string? Error { get; set; }
}

