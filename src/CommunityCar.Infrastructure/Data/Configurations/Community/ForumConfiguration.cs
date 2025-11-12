using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the Forum entity
/// </summary>
public class ForumConfiguration : BaseEntityConfiguration<Forum>
{
    /// <summary>
    /// Configures the Forum entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<Forum> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure required name property
        ConfigureRequiredStringProperty(
            builder,
            f => f.Name,
            EntityConfigurationConstants.MediumStringLength,
            EntityConfigurationConstants.NameProperty);

        // Configure required description property
        ConfigureRequiredStringProperty(
            builder,
            f => f.Description,
            EntityConfigurationConstants.ExtraLongStringLength,
            EntityConfigurationConstants.DescriptionProperty);

        // Configure optional icon URL property
        ConfigureOptionalStringProperty(
            builder,
            f => f.IconUrl,
            EntityConfigurationConstants.LongStringLength);

        // Configure display order with default value
        builder.Property(f => f.DisplayOrder)
            .HasDefaultValue(0);

        // Configure boolean property with index
        ConfigureBooleanPropertyWithIndex(
            builder,
            f => f.IsActive,
            "IsActive");

        // Configure index for display order
        builder.HasIndex(f => f.DisplayOrder)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Forum),
                "DisplayOrder"));
    }
}
