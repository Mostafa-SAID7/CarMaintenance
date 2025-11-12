namespace CommunityCar.Application.Interfaces;

public interface IRazorViewEngine
{
    Task<string> RenderViewToStringAsync(string viewName, object model);
}
