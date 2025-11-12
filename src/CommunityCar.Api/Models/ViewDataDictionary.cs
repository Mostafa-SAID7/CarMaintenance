namespace CommunityCar.Api.Models;

public class ViewDataDictionary : Dictionary<string, object?>
{
    private ModelMetadata? _modelMetadata;
    private object? _model;

    public ViewDataDictionary()
        : this(new EmptyModelMetadataProvider(), new ModelStateDictionary())
    {
    }

    public ViewDataDictionary(IModelMetadataProvider metadataProvider, ModelStateDictionary modelState)
    {
        ModelMetadata = metadataProvider.GetMetadataForType(typeof(object));
        ModelState = modelState ?? throw new ArgumentNullException(nameof(modelState));
    }

    public ViewDataDictionary(ViewDataDictionary dictionary)
        : base(dictionary)
    {
        _modelMetadata = dictionary._modelMetadata;
        _model = dictionary._model;
        ModelState = dictionary.ModelState;
    }

    public object? Model
    {
        get => _model;
        set
        {
            _model = value;
            if (_modelMetadata != null && _modelMetadata.ModelType != typeof(object))
            {
                // Update model metadata if the model type changes
                var metadataProvider = new EmptyModelMetadataProvider();
                _modelMetadata = metadataProvider.GetMetadataForType(_model?.GetType() ?? typeof(object));
            }
        }
    }

    public ModelMetadata ModelMetadata
    {
        get => _modelMetadata ??= new EmptyModelMetadata(typeof(object));
        set => _modelMetadata = value;
    }

    public ModelStateDictionary ModelState { get; set; } = new();

    public object? this[string key]
    {
        get => TryGetValue(key, out var value) ? value : null;
        set => base[key] = value;
    }

    public ViewDataDictionary Eval(string expression)
    {
        // Simple implementation - in real ASP.NET Core, this would parse the expression
        if (ContainsKey(expression))
        {
            var value = this[expression];
            var result = new ViewDataDictionary(this);
            result.Model = value;
            return result;
        }

        return new ViewDataDictionary();
    }

    public void Add(string key, object? value)
    {
        this[key] = value;
    }

    public bool ContainsKey(string key)
    {
        return base.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return base.Remove(key);
    }

    public void Clear()
    {
        base.Clear();
        _model = null;
    }
}

public class ModelStateDictionary : Dictionary<string, ModelStateEntry>
{
    public ModelStateDictionary()
    {
    }

    public ModelStateDictionary(ModelStateDictionary dictionary)
        : base(dictionary)
    {
    }

    public bool IsValid => !Values.Any(v => v.Errors.Any());

    public ModelErrorCollection Errors => new ModelErrorCollection(
        Values.SelectMany(v => v.Errors));

    public void AddModelError(string key, string errorMessage)
    {
        if (!TryGetValue(key, out var entry))
        {
            entry = new ModelStateEntry();
            this[key] = entry;
        }
        entry.Errors.Add(new ModelError(errorMessage));
    }

    public void AddModelError(string key, Exception exception)
    {
        if (!TryGetValue(key, out var entry))
        {
            entry = new ModelStateEntry();
            this[key] = entry;
        }
        entry.Errors.Add(new ModelError(exception));
    }

    public void Clear()
    {
        base.Clear();
    }

    public void Remove(string key)
    {
        base.Remove(key);
    }

    public void SetModelValue(string key, ValueProviderResult value)
    {
        if (!TryGetValue(key, out var entry))
        {
            entry = new ModelStateEntry();
            this[key] = entry;
        }
        entry.RawValue = value;
    }

    public bool TryGetValue(string key, out ValueProviderResult value)
    {
        if (TryGetValue(key, out var entry))
        {
            value = entry.RawValue;
            return true;
        }
        value = ValueProviderResult.None;
        return false;
    }
}

public class ModelStateEntry
{
    public ModelErrorCollection Errors { get; } = new();
    public ValueProviderResult RawValue { get; set; } = ValueProviderResult.None;
    public object? AttemptedValue => RawValue.FirstValue;
    public bool IsValid => !Errors.Any();
}

public class ModelErrorCollection : List<ModelError>
{
    public ModelErrorCollection()
    {
    }

    public ModelErrorCollection(IEnumerable<ModelError> errors)
        : base(errors)
    {
    }
}

public class ModelError
{
    public ModelError(string errorMessage)
    {
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
    }

    public ModelError(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        ErrorMessage = exception.Message;
    }

    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}

public class ValueProviderResult
{
    public static readonly ValueProviderResult None = new ValueProviderResult(new string[0], null);

    public ValueProviderResult(string[] values, string? culture)
    {
        Values = values ?? throw new ArgumentNullException(nameof(values));
        Culture = culture;
    }

    public ValueProviderResult(string value, string? culture)
        : this(new[] { value }, culture)
    {
    }

    public string[] Values { get; }
    public string? Culture { get; }
    public string? FirstValue => Values.Length > 0 ? Values[0] : null;

    public object? ConvertTo(Type type, Func<object?, object?>? converter = null)
    {
        var value = FirstValue;
        if (value == null)
            return null;

        if (converter != null)
            return converter(value);

        // Simple type conversion
        if (type == typeof(string))
            return value;
        if (type == typeof(int) && int.TryParse(value, out var intValue))
            return intValue;
        if (type == typeof(bool) && bool.TryParse(value, out var boolValue))
            return boolValue;
        if (type == typeof(DateTime) && DateTime.TryParse(value, out var dateValue))
            return dateValue;

        return value;
    }
}

public class ViewDataDictionary<TModel> : ViewDataDictionary
{
    public ViewDataDictionary()
        : base()
    {
    }

    public ViewDataDictionary(IModelMetadataProvider metadataProvider, ModelStateDictionary modelState)
        : base(metadataProvider, modelState)
    {
    }

    public ViewDataDictionary(ViewDataDictionary dictionary)
        : base(dictionary)
    {
    }

    public new TModel Model
    {
        get => (TModel)base.Model!;
        set => base.Model = value;
    }
}

public static class ViewDataDictionaryExtensions
{
    public static ViewDataDictionary<TModel> GetViewData<TModel>(this ViewDataDictionary viewData)
    {
        if (viewData is ViewDataDictionary<TModel> typedViewData)
            return typedViewData;

        return new ViewDataDictionary<TModel>(viewData);
    }

    public static T GetValue<T>(this ViewDataDictionary viewData, string key, T defaultValue = default!)
    {
        if (viewData.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;

        return defaultValue;
    }

    public static void SetValue(this ViewDataDictionary viewData, string key, object? value)
    {
        viewData[key] = value;
    }

    public static bool HasValue(this ViewDataDictionary viewData, string key)
    {
        return viewData.ContainsKey(key) && viewData[key] != null;
    }

    public static void RemoveValue(this ViewDataDictionary viewData, string key)
    {
        viewData.Remove(key);
    }
}