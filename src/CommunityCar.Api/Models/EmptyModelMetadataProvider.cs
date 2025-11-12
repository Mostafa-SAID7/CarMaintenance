using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace CommunityCar.Api.Models;

public class EmptyModelMetadataProvider : IModelMetadataProvider
{
    private readonly ModelMetadata[] _emptyMetadata = Array.Empty<ModelMetadata>();

    public IEnumerable<ModelMetadata> GetMetadataForProperties(Type modelType)
    {
        return _emptyMetadata;
    }

    public ModelMetadata GetMetadataForType(Type modelType)
    {
        return new EmptyModelMetadata(modelType);
    }

    public ModelMetadata GetMetadataForProperty(Type containerType, string propertyName)
    {
        return new EmptyModelMetadata(containerType);
    }
}

public class EmptyModelMetadata : ModelMetadata
{
    public EmptyModelMetadata(Type modelType)
        : base(ModelMetadataIdentity.ForType(modelType))
    {
    }

    public override IReadOnlyDictionary<object, object> AdditionalValues => new Dictionary<object, object>();

    public override ModelPropertyCollection Properties => ModelPropertyCollection.Empty;

    public override string? BinderModelName => null;

    public override Type? BinderType => null;

    public override BindingSource? BindingSource => null;

    public override bool ConvertEmptyStringToNull => false;

    public override string? DataTypeName => null;

    public override string? Description => null;

    public override string? DisplayFormatString => null;

    public override string? DisplayName => null;

    public override string? EditFormatString => null;

    public override IEnumerable<KeyValuePair<EnumGroupAndName, string>>? EnumGroupedDisplayNamesAndValues => null;

    public override string? EnumNamesAndValues => null;

    public override int? Order => null;

    public override string? Placeholder => null;

    public override bool IsBindingAllowed => true;

    public override bool IsBindingRequired => false;

    public override bool IsEnum => false;

    public override bool IsFlagsEnum => false;

    public override bool IsReadOnly => false;

    public override bool IsRequired => false;

    public override string? NullDisplayText => null;

    public override int? MaxLength => null;

    public override string? PropertyName => null;

    public override bool ShowForDisplay => true;

    public override bool ShowForEdit => true;

    public override string? SimpleDisplayProperty => null;

    public override string? TemplateHint => null;

    public override bool ValidateChildren => true;

    public override IPropertyValidationFilter? PropertyValidationFilter => null;

    public override bool HasValidators => false;

    public override IEnumerable<IModelValidator> ValidatorMetadata => Array.Empty<IModelValidator>();

    public override Func<object, object>? PropertyGetter => null;

    public override Action<object, object?>? PropertySetter => null;

    public override object? DefaultValue => null;
}

public interface IModelValidator
{
    string ErrorMessage { get; }
    bool IsRequired { get; }
}

public interface IPropertyValidationFilter
{
    bool ShouldValidateEntry(ValidationEntry entry, ValidationEntry parentEntry);
}

public class ValidationEntry
{
    public string PropertyName { get; set; } = string.Empty;
    public object? Model { get; set; }
    public ModelMetadata Metadata { get; set; } = null!;
}
