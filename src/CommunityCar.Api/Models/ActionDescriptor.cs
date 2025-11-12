using System.Reflection;

namespace CommunityCar.Api.Models;

public class ActionDescriptor
{
    public string? DisplayName { get; set; }
    public string? ActionName { get; set; }
    public string? ControllerName { get; set; }
    public MethodInfo? MethodInfo { get; set; }
    public Type? ControllerType { get; set; }
    public string? RouteTemplate { get; set; }
    public string? HttpMethod { get; set; }
    public Dictionary<string, object?> RouteValues { get; set; } = new();
    public Dictionary<string, object?> Properties { get; set; } = new();
    public List<string> HttpMethods { get; set; } = new();
    public List<string> SupportedMediaTypes { get; set; } = new();
    public List<ParameterDescriptor> Parameters { get; set; } = new();
    public List<FilterDescriptor> FilterDescriptors { get; set; } = new();
    public AttributeRouteInfo? AttributeRouteInfo { get; set; }
}

public class ParameterDescriptor
{
    public string Name { get; set; } = string.Empty;
    public Type ParameterType { get; set; } = typeof(object);
    public ParameterInfo? ParameterInfo { get; set; }
    public BindingInfo? BindingInfo { get; set; }
}

public class FilterDescriptor
{
    public int Order { get; set; }
    public object Filter { get; set; } = new object();
    public IFilterMetadata? FilterMetadata { get; set; }
}

public interface IFilterMetadata
{
}

public class AttributeRouteInfo
{
    public string? Template { get; set; }
    public string? Name { get; set; }
    public int? Order { get; set; }
    public bool SuppressLinkGeneration { get; set; }
    public bool SuppressPathMatching { get; set; }
}

public class BindingInfo
{
    public BindingSource? BindingSource { get; set; }
    public string? BinderModelName { get; set; }
    public Type? BinderType { get; set; }
    public bool IsTopLevel { get; set; }
    public string? ModelName { get; set; }
    public string? PropertyFilterProvider { get; set; }
    public Func<ModelMetadata, string?>? PropertyFilter { get; set; }
    public string? RequestPredicate { get; set; }
    public Func<ModelMetadata, bool>? RequestPredicateFunc { get; set; }
}

public class BindingSource
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsGreedy { get; set; }
    public bool IsFromRequest { get; set; }

    public static readonly BindingSource Query = new() { Id = "Query", DisplayName = "Query", IsGreedy = false, IsFromRequest = true };
    public static readonly BindingSource Form = new() { Id = "Form", DisplayName = "Form", IsGreedy = false, IsFromRequest = true };
    public static readonly BindingSource Path = new() { Id = "Path", DisplayName = "Path", IsGreedy = false, IsFromRequest = true };
    public static readonly BindingSource Header = new() { Id = "Header", DisplayName = "Header", IsGreedy = false, IsFromRequest = true };
    public static readonly BindingSource Body = new() { Id = "Body", DisplayName = "Body", IsGreedy = true, IsFromRequest = true };
    public static readonly BindingSource ModelBinding = new() { Id = "ModelBinding", DisplayName = "Model Binding", IsGreedy = false, IsFromRequest = false };
    public static readonly BindingSource Custom = new() { Id = "Custom", DisplayName = "Custom", IsGreedy = false, IsFromRequest = false };
}

public class ModelMetadata
{
    public Type ModelType { get; set; } = typeof(object);
    public string? PropertyName { get; set; }
    public string? DisplayName { get; set; }
    public bool IsRequired { get; set; }
    public object? DefaultValue { get; set; }
}