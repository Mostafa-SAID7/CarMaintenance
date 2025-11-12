using CommunityCar.Api.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Api.Controllers.Dashboard.Content;

[ApiController]
[Route("api/[controller]")]
public class LocalizationController : ControllerBase
{
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<LocalizationController> _logger;

    public LocalizationController(
        IStringLocalizer<SharedResource> localizer,
        ILogger<LocalizationController> logger)
    {
        _localizer = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Get all localized strings for the current culture
    /// </summary>
    [HttpGet("strings")]
    [ProducesResponseType(typeof(Dictionary<string, Dictionary<string, string>>), StatusCodes.Status200OK)]
    public IActionResult GetLocalizedStrings()
    {
        try
        {
            var localizedStrings = new Dictionary<string, Dictionary<string, string>>();

            // Common section
            localizedStrings["Common"] = new Dictionary<string, string>
            {
                ["Save"] = _localizer["Common.Save"],
                ["Cancel"] = _localizer["Common.Cancel"],
                ["Delete"] = _localizer["Common.Delete"],
                ["Edit"] = _localizer["Common.Edit"],
                ["Create"] = _localizer["Common.Create"],
                ["Update"] = _localizer["Common.Update"],
                ["Search"] = _localizer["Common.Search"],
                ["Filter"] = _localizer["Common.Filter"],
                ["Loading"] = _localizer["Common.Loading"],
                ["NoData"] = _localizer["Common.NoData"],
                ["Confirm"] = _localizer["Common.Confirm"],
                ["Yes"] = _localizer["Common.Yes"],
                ["No"] = _localizer["Common.No"],
                ["Close"] = _localizer["Common.Close"],
                ["Back"] = _localizer["Common.Back"],
                ["Next"] = _localizer["Common.Next"],
                ["Previous"] = _localizer["Common.Previous"],
                ["Submit"] = _localizer["Common.Submit"],
                ["Reset"] = _localizer["Common.Reset"],
                ["Refresh"] = _localizer["Common.Refresh"]
            };

            // Auth section
            localizedStrings["Auth"] = new Dictionary<string, string>
            {
                ["Login"] = _localizer["Auth.Login"],
                ["Logout"] = _localizer["Auth.Logout"],
                ["Register"] = _localizer["Auth.Register"],
                ["Email"] = _localizer["Auth.Email"],
                ["Password"] = _localizer["Auth.Password"],
                ["LoginSuccessful"] = _localizer["Auth.LoginSuccessful"],
                ["InvalidCredentials"] = _localizer["Auth.InvalidCredentials"],
                ["UserNotFound"] = _localizer["Auth.UserNotFound"],
                ["UserNotAuthenticated"] = _localizer["Auth.UserNotAuthenticated"]
            };

            // Profile section
            localizedStrings["Profile"] = new Dictionary<string, string>
            {
                ["Profile"] = _localizer["Profile.Profile"],
                ["MyProfile"] = _localizer["Profile.MyProfile"],
                ["EditProfile"] = _localizer["Profile.EditProfile"],
                ["ProfileUpdated"] = _localizer["Profile.ProfileUpdated"],
                ["FirstName"] = _localizer["Profile.FirstName"],
                ["LastName"] = _localizer["Profile.LastName"],
                ["InvalidProfileData"] = _localizer["Profile.InvalidProfileData"],
                ["ProfileNotFound"] = _localizer["Profile.ProfileNotFound"]
            };

            // Validation section
            localizedStrings["Validation"] = new Dictionary<string, string>
            {
                ["Required"] = _localizer["Validation.Required"],
                ["Email"] = _localizer["Validation.Email"],
                ["MinLength"] = _localizer["Validation.MinLength"],
                ["MaxLength"] = _localizer["Validation.MaxLength"],
                ["PasswordComplexity"] = _localizer["Validation.PasswordComplexity"]
            };

            // Errors section
            localizedStrings["Errors"] = new Dictionary<string, string>
            {
                ["Generic"] = _localizer["Errors.Generic"],
                ["Network"] = _localizer["Errors.Network"],
                ["Timeout"] = _localizer["Errors.Timeout"],
                ["NotFound"] = _localizer["Errors.NotFound"],
                ["BadRequest"] = _localizer["Errors.BadRequest"],
                ["Unauthorized"] = _localizer["Errors.Unauthorized"],
                ["InternalServerError"] = _localizer["Errors.InternalServerError"]
            };

            return Ok(new
            {
                culture = Thread.CurrentThread.CurrentCulture.Name,
                strings = localizedStrings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localized strings");
            return StatusCode(500, "Error retrieving localized strings");
        }
    }

    /// <summary>
    /// Get a specific localized string
    /// </summary>
    [HttpGet("string/{section}/{key}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetLocalizedString(string section, string key)
    {
        try
        {
            var fullKey = $"{section}.{key}";
            var localizedValue = _localizer[fullKey];

            if (string.IsNullOrEmpty(localizedValue))
            {
                return NotFound(new { message = $"Localization key '{fullKey}' not found" });
            }

            return Ok(new
            {
                key = fullKey,
                value = localizedValue,
                culture = Thread.CurrentThread.CurrentCulture.Name
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localized string for key {Section}.{Key}", section, key);
            return StatusCode(500, "Error retrieving localized string");
        }
    }

    /// <summary>
    /// Get supported cultures
    /// </summary>
    [HttpGet("cultures")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedCultures()
    {
        var cultures = new[]
        {
            new { code = "en", name = "English", nativeName = "English" },
            new { code = "ar", name = "Arabic", nativeName = "العربية" }
        };

        return Ok(new
        {
            currentCulture = Thread.CurrentThread.CurrentCulture.Name,
            supportedCultures = cultures
        });
    }

    /// <summary>
    /// Get localization statistics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetLocalizationStats()
    {
        try
        {
            // This would typically read from the localization files or cache
            // For now, return basic stats
            return Ok(new
            {
                totalSections = 10,
                totalKeys = 150,
                supportedCultures = new[] { "en", "ar" },
                currentCulture = Thread.CurrentThread.CurrentCulture.Name,
                lastUpdated = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localization statistics");
            return StatusCode(500, "Error retrieving localization statistics");
        }
    }

    /// <summary>
    /// Validate localization keys
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult ValidateLocalizationKeys([FromBody] IEnumerable<string> keys)
    {
        try
        {
            var results = new List<object>();

            foreach (var key in keys)
            {
                var localizedValue = _localizer[key];
                var isValid = !string.IsNullOrEmpty(localizedValue) && localizedValue != key;

                results.Add(new
                {
                    key,
                    isValid,
                    value = localizedValue,
                    culture = Thread.CurrentThread.CurrentCulture.Name
                });
            }

            return Ok(new
            {
                totalKeys = keys.Count(),
                validKeys = results.Count(r => (bool)r.GetType().GetProperty("isValid")?.GetValue(r) == true),
                invalidKeys = results.Count(r => (bool)r.GetType().GetProperty("isValid")?.GetValue(r) == false),
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating localization keys");
            return StatusCode(500, "Error validating localization keys");
        }
    }
}