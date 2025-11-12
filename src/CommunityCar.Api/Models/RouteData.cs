namespace CommunityCar.Api.Models;

public class RouteData
{
    private readonly Dictionary<string, object> _dataTokens = new();
    private readonly Dictionary<string, object> _values = new();

    public string? RouteName { get; set; }

    public RouteValueDictionary Values => new(_values);

    public RouteValueDictionary DataTokens => new(_dataTokens);

    public IRouter? Router { get; set; }

    public RouteData()
    {
    }

    public RouteData(RouteData routeData)
    {
        RouteName = routeData.RouteName;
        Router = routeData.Router;

        foreach (var kvp in routeData._values)
        {
            _values[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in routeData._dataTokens)
        {
            _dataTokens[kvp.Key] = kvp.Value;
        }
    }

    public object? this[string key]
    {
        get => _values.TryGetValue(key, out var value) ? value : null;
        set => _values[key] = value!;
    }

    public void Add(string key, object value)
    {
        _values[key] = value;
    }

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _values.Remove(key);
    }

    public void Clear()
    {
        _values.Clear();
        _dataTokens.Clear();
    }

    public IEnumerable<string> Keys => _values.Keys;

    public IEnumerable<object> Values => _values.Values;

    public int Count => _values.Count;

    public bool TryGetValue(string key, out object value)
    {
        return _values.TryGetValue(key, out value);
    }
}

public class RouteValueDictionary : Dictionary<string, object?>
{
    public RouteValueDictionary()
    {
    }

    public RouteValueDictionary(IDictionary<string, object?> dictionary)
        : base(dictionary)
    {
    }

    public RouteValueDictionary(object values)
    {
        if (values != null)
        {
            var properties = values.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(values);
                this[property.Name] = value;
            }
        }
    }
}

public interface IRouter
{
    Task RouteAsync(RouteContext context);
    VirtualPathData? GetVirtualPath(VirtualPathContext context);
}

public class RouteContext
{
    public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    public RouteData RouteData { get; set; } = new RouteData();
    public bool IsHandled { get; set; }
}

public class VirtualPathContext
{
    public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    public RouteValueDictionary Values { get; set; } = new();
    public RouteValueDictionary AmbientValues { get; set; } = new();
    public string? RouteName { get; set; }
}

public class VirtualPathData
{
    public IRouter? Router { get; set; }
    public RouteValueDictionary DataTokens { get; set; } = new();
    public string VirtualPath { get; set; } = string.Empty;
}

public class RouteEndpoint
{
    public string RoutePattern { get; set; } = string.Empty;
    public RouteValueDictionary Defaults { get; set; } = new();
    public RouteValueDictionary Constraints { get; set; } = new();
    public RouteValueDictionary DataTokens { get; set; } = new();
    public int Order { get; set; }
    public string? DisplayName { get; set; }
    public EndpointMetadataCollection Metadata { get; set; } = new();
}

public class EndpointMetadataCollection : List<object>
{
    public EndpointMetadataCollection()
    {
    }

    public EndpointMetadataCollection(IEnumerable<object> items)
        : base(items)
    {
    }

    public T? GetMetadata<T>() where T : class
    {
        return this.OfType<T>().FirstOrDefault();
    }

    public IEnumerable<T> GetMetadata<T>() where T : class
    {
        return this.OfType<T>();
    }
}

public class RouteAttribute : Attribute
{
    public string Template { get; }

    public RouteAttribute(string template)
    {
        Template = template;
    }
}

public class HttpMethodAttribute : Attribute
{
    public IEnumerable<string> HttpMethods { get; }

    public HttpMethodAttribute(params string[] httpMethods)
    {
        HttpMethods = httpMethods;
    }
}

public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string template = null) : base("GET")
    {
        Template = template;
    }

    public string? Template { get; }
}

public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string template = null) : base("POST")
    {
        Template = template;
    }

    public string? Template { get; }
}

public class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute(string template = null) : base("PUT")
    {
        Template = template;
    }

    public string? Template { get; }
}

public class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute(string template = null) : base("DELETE")
    {
        Template = template;
    }

    public string? Template { get; }
}

public class HttpPatchAttribute : HttpMethodAttribute
{
    public HttpPatchAttribute(string template = null) : base("PATCH")
    {
        Template = template;
    }

    public string? Template { get; }
}

public class RouteConstraint
{
    public string ConstraintName { get; set; } = string.Empty;
    public string ConstraintValue { get; set; } = string.Empty;
}

public class RouteConstraints : Dictionary<string, RouteConstraint>
{
    public RouteConstraints()
    {
    }

    public RouteConstraints(IDictionary<string, RouteConstraint> dictionary)
        : base(dictionary)
    {
    }
}