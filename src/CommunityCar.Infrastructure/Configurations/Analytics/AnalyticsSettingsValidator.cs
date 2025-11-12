using Microsoft.Extensions.Options;

namespace CommunityCar.Infrastructure.Configurations.Analytics;

/// <summary>
/// Validator for AnalyticsSettings using the Options pattern
/// </summary>
public class AnalyticsSettingsValidator : IValidateOptions<AnalyticsSettings>
{
    /// <summary>
    /// Validates the analytics settings options
    /// </summary>
    /// <param name="name">The name of the options instance</param>
    /// <param name="options">The options instance to validate</param>
    /// <returns>Success if valid, otherwise failure with details</returns>
    public ValidateOptionsResult Validate(string? name, AnalyticsSettings options)
    {
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            options.Validate();
            return ValidateOptionsResult.Success;
        }
        catch (ValidationException ex)
        {
            return ValidateOptionsResult.Fail($"Invalid analytics configuration for '{name}': {ex.Message}");
        }
        catch (Exception ex)
        {
            return ValidateOptionsResult.Fail($"Unexpected error validating analytics settings for '{name}': {ex.Message}");
        }
    }
}
