namespace CommunityCar.Api.Models;

public class TempDataDictionary : Dictionary<string, object?>, ITempDataDictionary
{
    private readonly ITempDataProvider _provider;
    private readonly HttpContext _httpContext;
    private bool _isLoaded;

    public TempDataDictionary(HttpContext httpContext, ITempDataProvider provider)
    {
        _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public void Load()
    {
        if (!_isLoaded)
        {
            var tempData = _provider.LoadTempData(_httpContext);
            if (tempData != null)
            {
                foreach (var kvp in tempData)
                {
                    this[kvp.Key] = kvp.Value;
                }
            }
            _isLoaded = true;
        }
    }

    public void Save()
    {
        _provider.SaveTempData(_httpContext, this);
    }

    public void Keep()
    {
        _provider.SaveTempData(_httpContext, this);
    }

    public void Keep(string key)
    {
        if (ContainsKey(key))
        {
            var value = this[key];
            _provider.SaveTempData(_httpContext, new Dictionary<string, object?> { [key] = value });
        }
    }

    public void Peek(string key, object? value)
    {
        this[key] = value;
        _provider.SaveTempData(_httpContext, new Dictionary<string, object?> { [key] = value });
    }

    public object? Peek(string key)
    {
        return this[key];
    }

    public void Clear()
    {
        base.Clear();
        _provider.SaveTempData(_httpContext, this);
    }

    public new void Remove(string key)
    {
        base.Remove(key);
        _provider.SaveTempData(_httpContext, this);
    }
}

public interface ITempDataDictionary : IDictionary<string, object?>
{
    void Load();
    void Save();
    void Keep();
    void Keep(string key);
    void Peek(string key, object? value);
    object? Peek(string key);
}

public interface ITempDataProvider
{
    IDictionary<string, object?> LoadTempData(HttpContext httpContext);
    void SaveTempData(HttpContext httpContext, IDictionary<string, object?> values);
}

public class SessionTempDataProvider : ITempDataProvider
{
    private const string TempDataSessionKey = "__ControllerTempData";

    public IDictionary<string, object?> LoadTempData(HttpContext httpContext)
    {
        var session = httpContext.Session;
        if (session.TryGetValue(TempDataSessionKey, out var bytes))
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(bytes);
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();
            }
            catch
            {
                return new Dictionary<string, object?>();
            }
        }

        return new Dictionary<string, object?>();
    }

    public void SaveTempData(HttpContext httpContext, IDictionary<string, object?> values)
    {
        var session = httpContext.Session;
        if (values.Any())
        {
            var json = System.Text.Json.JsonSerializer.Serialize(values);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            session.Set(TempDataSessionKey, bytes);
        }
        else
        {
            session.Remove(TempDataSessionKey);
        }
    }
}

public class CookieTempDataProvider : ITempDataProvider
{
    private const string TempDataCookieName = ".AspNetCore.Mvc.CookieTempDataProvider";
    private readonly ICookieManager _cookieManager;

    public CookieTempDataProvider(ICookieManager cookieManager)
    {
        _cookieManager = cookieManager;
    }

    public IDictionary<string, object?> LoadTempData(HttpContext httpContext)
    {
        var cookieValue = _cookieManager.GetRequestCookie(httpContext, TempDataCookieName);
        if (!string.IsNullOrEmpty(cookieValue))
        {
            try
            {
                var decodedValue = Convert.FromBase64String(cookieValue);
                var json = System.Text.Encoding.UTF8.GetString(decodedValue);
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();
            }
            catch
            {
                return new Dictionary<string, object?>();
            }
        }

        return new Dictionary<string, object?>();
    }

    public void SaveTempData(HttpContext httpContext, IDictionary<string, object?> values)
    {
        if (values.Any())
        {
            var json = System.Text.Json.JsonSerializer.Serialize(values);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var encodedValue = Convert.ToBase64String(bytes);
            _cookieManager.AppendResponseCookie(httpContext, TempDataCookieName, encodedValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
        }
        else
        {
            _cookieManager.DeleteCookie(httpContext, TempDataCookieName);
        }
    }
}

public interface ICookieManager
{
    string? GetRequestCookie(HttpContext httpContext, string key);
    void AppendResponseCookie(HttpContext httpContext, string key, string value, CookieOptions options);
    void DeleteCookie(HttpContext httpContext, string key);
}

public class DefaultCookieManager : ICookieManager
{
    public string? GetRequestCookie(HttpContext httpContext, string key)
    {
        return httpContext.Request.Cookies[key];
    }

    public void AppendResponseCookie(HttpContext httpContext, string key, string value, CookieOptions options)
    {
        httpContext.Response.Cookies.Append(key, value, options);
    }

    public void DeleteCookie(HttpContext httpContext, string key)
    {
        httpContext.Response.Cookies.Delete(key);
    }
}

public class TempDataSerializer
{
    public static string Serialize(IDictionary<string, object?> data)
    {
        return System.Text.Json.JsonSerializer.Serialize(data);
    }

    public static IDictionary<string, object?> Deserialize(string data)
    {
        return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(data) ?? new Dictionary<string, object?>();
    }
}

public static class TempDataExtensions
{
    public static void AddMessage(this ITempDataDictionary tempData, string key, string message)
    {
        tempData[key] = message;
    }

    public static string? GetMessage(this ITempDataDictionary tempData, string key)
    {
        var message = tempData[key]?.ToString();
        if (!string.IsNullOrEmpty(message))
        {
            tempData.Remove(key);
            return message;
        }
        return null;
    }

    public static void AddSuccessMessage(this ITempDataDictionary tempData, string message)
    {
        tempData.AddMessage("SuccessMessage", message);
    }

    public static string? GetSuccessMessage(this ITempDataDictionary tempData)
    {
        return tempData.GetMessage("SuccessMessage");
    }

    public static void AddErrorMessage(this ITempDataDictionary tempData, string message)
    {
        tempData.AddMessage("ErrorMessage", message);
    }

    public static string? GetErrorMessage(this ITempDataDictionary tempData)
    {
        return tempData.GetMessage("ErrorMessage");
    }

    public static void AddWarningMessage(this ITempDataDictionary tempData, string message)
    {
        tempData.AddMessage("WarningMessage", message);
    }

    public static string? GetWarningMessage(this ITempDataDictionary tempData)
    {
        return tempData.GetMessage("WarningMessage");
    }

    public static void AddInfoMessage(this ITempDataDictionary tempData, string message)
    {
        tempData.AddMessage("InfoMessage", message);
    }

    public static string? GetInfoMessage(this ITempDataDictionary tempData)
    {
        return tempData.GetMessage("InfoMessage");
    }
}
