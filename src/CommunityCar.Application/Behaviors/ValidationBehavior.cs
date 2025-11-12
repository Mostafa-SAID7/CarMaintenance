using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Skip validation for requests that don't need it
        if (request is ISkipValidation)
        {
            _logger.LogDebug("Skipping validation for {RequestName}", requestName);
            return await next();
        }

        // Perform validation
        var validationContext = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(validationContext, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning("Validation failed for {RequestName}: {ErrorCount} errors",
                requestName, failures.Count);

            // Log validation errors
            foreach (var failure in failures)
            {
                _logger.LogWarning("Validation error in {RequestName}: {Property} - {Error}",
                    requestName, failure.PropertyName, failure.ErrorMessage);
            }

            // Create validation exception with detailed errors
            var validationException = new ValidationException(
                $"Validation failed for {requestName}",
                failures.Select(f => new ValidationFailure(f.PropertyName, f.ErrorMessage, f.AttemptedValue)));

            throw validationException;
        }

        _logger.LogDebug("Validation passed for {RequestName}", requestName);
        return await next();
    }
}

public interface ISkipValidation
{
    // Marker interface for requests that should skip validation
}

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = new List<ValidationFailure>();
    }

    public ValidationException(string message, IEnumerable<ValidationFailure> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new List<ValidationFailure>();
    }
}

public class ValidationFailure
{
    public string PropertyName { get; }
    public string ErrorMessage { get; }
    public object? AttemptedValue { get; }
    public string? ErrorCode { get; }
    public Severity Severity { get; }
    public Dictionary<string, object> CustomState { get; }

    public ValidationFailure(string propertyName, string errorMessage)
        : this(propertyName, errorMessage, null)
    {
    }

    public ValidationFailure(string propertyName, string errorMessage, object? attemptedValue)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
        AttemptedValue = attemptedValue;
        CustomState = new Dictionary<string, object>();
        Severity = Severity.Error;
    }
}

public enum Severity
{
    Error,
    Warning,
    Info
}

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public IList<ValidationFailure> Errors { get; } = new List<ValidationFailure>();
    public string? RuleSet { get; set; }

    public ValidationResult()
    {
    }

    public ValidationResult(IEnumerable<ValidationFailure> errors)
    {
        Errors = errors.ToList();
    }

    public void AddError(string propertyName, string errorMessage)
    {
        Errors.Add(new ValidationFailure(propertyName, errorMessage));
    }

    public void AddError(string propertyName, string errorMessage, object attemptedValue)
    {
        Errors.Add(new ValidationFailure(propertyName, errorMessage, attemptedValue));
    }
}

public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly List<IValidationRule<T>> _rules = new();

    public ValidationResult Validate(T instance)
    {
        return Validate(new ValidationContext<T>(instance));
    }

    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default)
    {
        return ValidateAsync(new ValidationContext<T>(instance), cancellation);
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();

        foreach (var rule in _rules)
        {
            var ruleResult = rule.Validate(context);
            if (!ruleResult.IsValid)
            {
                foreach (var error in ruleResult.Errors)
                {
                    result.Errors.Add(error);
                }
            }
        }

        return result;
    }

    public async Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        var result = new ValidationResult();

        foreach (var rule in _rules)
        {
            var ruleResult = await rule.ValidateAsync(context, cancellation);
            if (!ruleResult.IsValid)
            {
                foreach (var error in ruleResult.Errors)
                {
                    result.Errors.Add(error);
                }
            }
        }

        return result;
    }

    protected IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var rule = new ValidationRule<T, TProperty>(expression);
        _rules.Add(rule);
        return new RuleBuilder<T, TProperty>(rule);
    }

    protected IRuleBuilderInitialCollection<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> expression)
    {
        var rule = new CollectionValidationRule<T, TElement>(expression);
        _rules.Add(rule);
        return new CollectionRuleBuilder<T, TElement>(rule);
    }
}

public interface IValidator<T>
{
    ValidationResult Validate(T instance);
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);
    ValidationResult Validate(ValidationContext<T> context);
    Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default);
}

public class ValidationContext<T>
{
    public T InstanceToValidate { get; }
    public string? PropertyName { get; }
    public string? DisplayName { get; }
    public object? RootContextData { get; set; }
    public Dictionary<string, object> ContextData { get; } = new();

    public ValidationContext(T instance)
    {
        InstanceToValidate = instance ?? throw new ArgumentNullException(nameof(instance));
    }

    public ValidationContext(T instance, string propertyName)
        : this(instance)
    {
        PropertyName = propertyName;
    }
}

public interface IValidationRule<T>
{
    ValidationResult Validate(ValidationContext<T> context);
    Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default);
}

public class ValidationRule<T, TProperty> : IValidationRule<T>
{
    private readonly Expression<Func<T, TProperty>> _expression;
    private readonly List<IValidationRule<TProperty>> _propertyRules = new();

    public ValidationRule(Expression<Func<T, TProperty>> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var value = _expression.Compile()(context.InstanceToValidate);
        var propertyContext = new ValidationContext<TProperty>(value);

        var result = new ValidationResult();
        foreach (var rule in _propertyRules)
        {
            var ruleResult = rule.Validate(propertyContext);
            if (!ruleResult.IsValid)
            {
                foreach (var error in ruleResult.Errors)
                {
                    result.Errors.Add(new ValidationFailure(
                        GetPropertyName(),
                        error.ErrorMessage,
                        value));
                }
            }
        }

        return result;
    }

    public async Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        var value = _expression.Compile()(context.InstanceToValidate);
        var propertyContext = new ValidationContext<TProperty>(value);

        var result = new ValidationResult();
        foreach (var rule in _propertyRules)
        {
            var ruleResult = await rule.ValidateAsync(propertyContext, cancellation);
            if (!ruleResult.IsValid)
            {
                foreach (var error in ruleResult.Errors)
                {
                    result.Errors.Add(new ValidationFailure(
                        GetPropertyName(),
                        error.ErrorMessage,
                        value));
                }
            }
        }

        return result;
    }

    private string GetPropertyName()
    {
        var member = _expression.Body as MemberExpression;
        return member?.Member.Name ?? "Unknown";
    }

    public void AddRule(IValidationRule<TProperty> rule)
    {
        _propertyRules.Add(rule);
    }
}

public class CollectionValidationRule<T, TElement> : IValidationRule<T>
{
    private readonly Expression<Func<T, IEnumerable<TElement>>> _expression;
    private readonly List<IValidationRule<TElement>> _elementRules = new();

    public CollectionValidationRule(Expression<Func<T, IEnumerable<TElement>>> expression)
    {
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var collection = _expression.Compile()(context.InstanceToValidate);
        var result = new ValidationResult();

        if (collection != null)
        {
            var index = 0;
            foreach (var element in collection)
            {
                var elementContext = new ValidationContext<TElement>(element);
                foreach (var rule in _elementRules)
                {
                    var ruleResult = rule.Validate(elementContext);
                    if (!ruleResult.IsValid)
                    {
                        foreach (var error in ruleResult.Errors)
                        {
                            result.Errors.Add(new ValidationFailure(
                                $"{GetPropertyName()}[{index}].{error.PropertyName}",
                                error.ErrorMessage,
                                element));
                        }
                    }
                }
                index++;
            }
        }

        return result;
    }

    public async Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        var collection = _expression.Compile()(context.InstanceToValidate);
        var result = new ValidationResult();

        if (collection != null)
        {
            var index = 0;
            foreach (var element in collection)
            {
                var elementContext = new ValidationContext<TElement>(element);
                foreach (var rule in _elementRules)
                {
                    var ruleResult = await rule.ValidateAsync(elementContext, cancellation);
                    if (!ruleResult.IsValid)
                    {
                        foreach (var error in ruleResult.Errors)
                        {
                            result.Errors.Add(new ValidationFailure(
                                $"{GetPropertyName()}[{index}].{error.PropertyName}",
                                error.ErrorMessage,
                                element));
                        }
                    }
                }
                index++;
            }
        }

        return result;
    }

    private string GetPropertyName()
    {
        var member = _expression.Body as MemberExpression;
        return member?.Member.Name ?? "Unknown";
    }

    public void AddRule(IValidationRule<TElement> rule)
    {
        _elementRules.Add(rule);
    }
}

public interface IRuleBuilderInitial<T, TProperty>
{
    IRuleBuilderOptions<T, TProperty> NotNull();
    IRuleBuilderOptions<T, TProperty> NotEmpty();
    IRuleBuilderOptions<T, TProperty> Length(int min, int max);
    IRuleBuilderOptions<T, TProperty> MaximumLength(int max);
    IRuleBuilderOptions<T, TProperty> MinimumLength(int min);
    IRuleBuilderOptions<T, TProperty> EmailAddress();
    IRuleBuilderOptions<T, TProperty> Matches(string expression);
    IRuleBuilderOptions<T, TProperty> Must(Func<TProperty, bool> predicate);
    IRuleBuilderOptions<T, TProperty> Equal(TProperty valueToCompare);
    IRuleBuilderOptions<T, TProperty> GreaterThan(TProperty valueToCompare);
    IRuleBuilderOptions<T, TProperty> LessThan(TProperty valueToCompare);
}

public interface IRuleBuilderOptions<T, TProperty>
{
    IRuleBuilderOptions<T, TProperty> WithMessage(string message);
    IRuleBuilderOptions<T, TProperty> WithErrorCode(string errorCode);
    IRuleBuilderOptions<T, TProperty> WithSeverity(Severity severity);
    IRuleBuilderOptions<T, TProperty> When(Func<T, bool> predicate);
    IRuleBuilderOptions<T, TProperty> Unless(Func<T, bool> predicate);
}

public interface IRuleBuilderInitialCollection<T, TElement>
{
    IRuleBuilderOptionsConditions<T, TElement> NotNull();
    IRuleBuilderOptionsConditions<T, TElement> NotEmpty();
    IRuleBuilderOptionsConditions<T, TElement> Must(Func<IEnumerable<TElement>, bool> predicate);
}

public interface IRuleBuilderOptionsConditions<T, TElement>
{
    IRuleBuilderOptionsConditions<T, TElement> WithMessage(string message);
    IRuleBuilderOptionsConditions<T, TElement> When(Func<T, bool> predicate);
}

public class RuleBuilder<T, TProperty> : IRuleBuilderInitial<T, TProperty>, IRuleBuilderOptions<T, TProperty>
{
    private readonly ValidationRule<T, TProperty> _rule;

    public RuleBuilder(ValidationRule<T, TProperty> rule)
    {
        _rule = rule;
    }

    public IRuleBuilderOptions<T, TProperty> NotNull()
    {
        _rule.AddRule(new NotNullRule<TProperty>());
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> NotEmpty()
    {
        _rule.AddRule(new NotEmptyRule<TProperty>());
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> Length(int min, int max)
    {
        _rule.AddRule(new LengthRule<TProperty>(min, max));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> MaximumLength(int max)
    {
        _rule.AddRule(new MaximumLengthRule<TProperty>(max));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> MinimumLength(int min)
    {
        _rule.AddRule(new MinimumLengthRule<TProperty>(min));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> EmailAddress()
    {
        _rule.AddRule(new EmailAddressRule<TProperty>());
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> Matches(string expression)
    {
        _rule.AddRule(new RegularExpressionRule<TProperty>(expression));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> Must(Func<TProperty, bool> predicate)
    {
        _rule.AddRule(new PredicateRule<TProperty>(predicate));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> Equal(TProperty valueToCompare)
    {
        _rule.AddRule(new EqualRule<TProperty>(valueToCompare));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> GreaterThan(TProperty valueToCompare)
    {
        _rule.AddRule(new GreaterThanRule<TProperty>(valueToCompare));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> LessThan(TProperty valueToCompare)
    {
        _rule.AddRule(new LessThanRule<TProperty>(valueToCompare));
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> WithMessage(string message)
    {
        // Implementation would set custom message
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> WithErrorCode(string errorCode)
    {
        // Implementation would set error code
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> WithSeverity(Severity severity)
    {
        // Implementation would set severity
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> When(Func<T, bool> predicate)
    {
        // Implementation would add condition
        return this;
    }

    public IRuleBuilderOptions<T, TProperty> Unless(Func<T, bool> predicate)
    {
        return When(x => !predicate(x));
    }
}

public class CollectionRuleBuilder<T, TElement> : IRuleBuilderInitialCollection<T, TElement>, IRuleBuilderOptionsConditions<T, TElement>
{
    private readonly CollectionValidationRule<T, TElement> _rule;

    public CollectionRuleBuilder(CollectionValidationRule<T, TElement> rule)
    {
        _rule = rule;
    }

    public IRuleBuilderOptionsConditions<T, TElement> NotNull()
    {
        // Implementation for collection not null
        return this;
    }

    public IRuleBuilderOptionsConditions<T, TElement> NotEmpty()
    {
        // Implementation for collection not empty
        return this;
    }

    public IRuleBuilderOptionsConditions<T, TElement> Must(Func<IEnumerable<TElement>, bool> predicate)
    {
        // Implementation for collection predicate
        return this;
    }

    public IRuleBuilderOptionsConditions<T, TElement> WithMessage(string message)
    {
        // Implementation would set custom message
        return this;
    }

    public IRuleBuilderOptionsConditions<T, TElement> When(Func<T, bool> predicate)
    {
        // Implementation would add condition
        return this;
    }
}

// Basic validation rule implementations
public class NotNullRule<T> : IValidationRule<T>
{
    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate == null)
        {
            result.AddError("", "Value cannot be null");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class NotEmptyRule<T> : IValidationRule<T>
{
    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate == null)
        {
            result.AddError("", "Value cannot be null or empty");
        }
        else if (context.InstanceToValidate is string str && string.IsNullOrWhiteSpace(str))
        {
            result.AddError("", "Value cannot be null or empty");
        }
        else if (context.InstanceToValidate is IEnumerable<object> collection && !collection.Any())
        {
            result.AddError("", "Collection cannot be empty");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class LengthRule<T> : IValidationRule<T>
{
    private readonly int _min;
    private readonly int _max;

    public LengthRule(int min, int max)
    {
        _min = min;
        _max = max;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate is string str)
        {
            if (str.Length < _min || str.Length > _max)
            {
                result.AddError("", $"Length must be between {_min} and {_max} characters");
            }
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class MaximumLengthRule<T> : IValidationRule<T>
{
    private readonly int _max;

    public MaximumLengthRule(int max)
    {
        _max = max;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate is string str && str.Length > _max)
        {
            result.AddError("", $"Maximum length is {_max} characters");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class MinimumLengthRule<T> : IValidationRule<T>
{
    private readonly int _min;

    public MinimumLengthRule(int min)
    {
        _min = min;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate is string str && str.Length < _min)
        {
            result.AddError("", $"Minimum length is {_min} characters");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class EmailAddressRule<T> : IValidationRule<T>
{
    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate is string email && !string.IsNullOrEmpty(email))
        {
            // Simple email validation
            if (!email.Contains("@") || !email.Contains("."))
            {
                result.AddError("", "Invalid email address format");
            }
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class RegularExpressionRule<T> : IValidationRule<T>
{
    private readonly string _pattern;

    public RegularExpressionRule(string pattern)
    {
        _pattern = pattern;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (context.InstanceToValidate is string str && !string.IsNullOrEmpty(str))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(str, _pattern))
            {
                result.AddError("", "Value does not match required pattern");
            }
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class PredicateRule<T> : IValidationRule<T>
{
    private readonly Func<T, bool> _predicate;

    public PredicateRule(Func<T, bool> predicate)
    {
        _predicate = predicate;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (!_predicate(context.InstanceToValidate))
        {
            result.AddError("", "Validation failed");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class EqualRule<T> : IValidationRule<T>
{
    private readonly T _valueToCompare;

    public EqualRule(T valueToCompare)
    {
        _valueToCompare = valueToCompare;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (!EqualityComparer<T>.Default.Equals(context.InstanceToValidate, _valueToCompare))
        {
            result.AddError("", "Values do not match");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class GreaterThanRule<T> : IValidationRule<T>
{
    private readonly T _valueToCompare;

    public GreaterThanRule(T valueToCompare)
    {
        _valueToCompare = valueToCompare;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (Comparer<T>.Default.Compare(context.InstanceToValidate, _valueToCompare) <= 0)
        {
            result.AddError("", "Value must be greater than comparison value");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public class LessThanRule<T> : IValidationRule<T>
{
    private readonly T _valueToCompare;

    public LessThanRule(T valueToCompare)
    {
        _valueToCompare = valueToCompare;
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var result = new ValidationResult();
        if (Comparer<T>.Default.Compare(context.InstanceToValidate, _valueToCompare) >= 0)
        {
            result.AddError("", "Value must be less than comparison value");
        }
        return result;
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        return Task.FromResult(Validate(context));
    }
}

public static class ValidationExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.AddBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }

    public static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        // Implementation would scan assembly for validators and register them
        return services;
    }
}
