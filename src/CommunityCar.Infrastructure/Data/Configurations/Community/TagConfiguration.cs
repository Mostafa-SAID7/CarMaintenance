using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the Tag entity
/// </summary>
public class TagConfiguration : BaseEntityConfiguration<Tag>
{
    /// <summary>
    /// Configures the Tag entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure required name property
        ConfigureRequiredStringProperty(
            builder,
            t => t.Name,
            EntityConfigurationConstants.ShortStringLength,
            EntityConfigurationConstants.NameProperty);

        // Configure optional description property
        ConfigureOptionalStringProperty(
            builder,
            t => t.Description,
            EntityConfigurationConstants.MediumStringLength);

        // Configure usage count with default value
        builder.Property(t => t.UsageCount)
            .HasDefaultValue(0);

        // Configure boolean property with index
        ConfigureBooleanPropertyWithIndex(
            builder,
            t => t.IsApproved,
            "IsApproved");

        // Configure index for usage count
        builder.HasIndex(t => t.UsageCount)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Tag),
                "UsageCount"));
    }
}
