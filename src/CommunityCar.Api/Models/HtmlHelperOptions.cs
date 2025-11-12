namespace CommunityCar.Api.Models;

public class HtmlHelperOptions
{
    public string IdAttributeDotReplacement { get; set; } = "_";
    public string ValidationInputCssClassName { get; set; } = "input-validation-error";
    public string ValidationInputValidCssClassName { get; set; } = "input-validation-valid";
    public string ValidationMessageCssClassName { get; set; } = "field-validation-error";
    public string ValidationMessageValidCssClassName { get; set; } = "field-validation-valid";
    public string ValidationSummaryCssClassName { get; set; } = "validation-summary-errors";
    public string ValidationSummaryValidCssClassName { get; set; } = "validation-summary-valid";
    public bool ClientValidationEnabled { get; set; } = true;
    public bool UnobtrusiveJavaScriptEnabled { get; set; } = true;
    public string Html5DateInputFormat { get; set; } = "yyyy-MM-dd";
    public string Html5DateTimeLocalInputFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss";
    public string Html5DateTimeInputFormat { get; set; } = "yyyy-MM-ddTHH:mm:ss.fffK";
    public string Html5TimeInputFormat { get; set; } = "HH:mm:ss";
    public bool HideMethodNotAllowedResponses { get; set; } = false;
    public bool CheckBoxHiddenInputRenderMode { get; set; } = false;
}

public class TagHelperOptions
{
    public string IdAttributeDotReplacement { get; set; } = "_";
    public string ValidationInputCssClassName { get; set; } = "input-validation-error";
    public string ValidationInputValidCssClassName { get; set; } = "input-validation-valid";
    public string ValidationMessageCssClassName { get; set; } = "field-validation-error";
    public string ValidationMessageValidCssClassName { get; set; } = "field-validation-valid";
    public string ValidationSummaryCssClassName { get; set; } = "validation-summary-errors";
    public string ValidationSummaryValidCssClassName { get; set; } = "validation-summary-valid";
}

public class FormContext
{
    public string? FormId { get; set; }
    public bool HasValidationErrors { get; set; }
    public bool CanRenderAtEndOfForm { get; set; }
    public Dictionary<string, object?> FormData { get; set; } = new();
    public List<FormField> Fields { get; set; } = new();
}

public class FormField
{
    public string Name { get; set; } = string.Empty;
    public string? Label { get; set; }
    public string? Value { get; set; }
    public bool IsRequired { get; set; }
    public string? ValidationMessage { get; set; }
    public bool HasValidationError { get; set; }
    public Dictionary<string, object?> Attributes { get; set; } = new();
}

public class HtmlEncoderOptions
{
    public bool UseCharacterEntityReferences { get; set; } = false;
    public HashSet<char>? CharactersToEncode { get; set; }
}

public class JsonSerializerOptions
{
    public bool WriteIndented { get; set; } = false;
    public bool IgnoreNullValues { get; set; } = false;
    public bool PropertyNameCaseInsensitive { get; set; } = false;
    public Dictionary<string, object?> Converters { get; set; } = new();
}

public class ViewEngineOptions
{
    public List<string> ViewLocationFormats { get; set; } = new()
    {
        "/Views/{1}/{0}.cshtml",
        "/Views/Shared/{0}.cshtml"
    };

    public List<string> AreaViewLocationFormats { get; set; } = new()
    {
        "/Areas/{2}/Views/{1}/{0}.cshtml",
        "/Areas/{2}/Views/Shared/{0}.cshtml",
        "/Views/Shared/{0}.cshtml"
    };

    public List<string> PageViewLocationFormats { get; set; } = new()
    {
        "/Pages/{1}/{0}.cshtml",
        "/Pages/Shared/{0}.cshtml"
    };
}

public class RazorViewEngineOptions : ViewEngineOptions
{
    public string AreaViewLocationFormatsSeparator { get; set; } = "/";
    public bool AllowRecompilingViewsOnFileChange { get; set; } = true;
    public List<IRazorViewEngineFeature> ViewEngineFeatures { get; set; } = new();
}

public interface IRazorViewEngineFeature
{
    string Name { get; }
    bool IsEnabled { get; set; }
}