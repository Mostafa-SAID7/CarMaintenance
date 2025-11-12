using CommunityCar.Domain.Entities.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the ForumCategory entity
/// </summary>
public class ForumCategoryConfiguration : BaseEntityConfiguration<ForumCategory>
{
    /// <summary>
    /// Configures the ForumCategory entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<ForumCategory> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure required name property (using 150 as specific length)
        ConfigureRequiredStringProperty(
            builder,
            fc => fc.Name,
            150,
            EntityConfigurationConstants.NameProperty);

        // Configure optional description property
        ConfigureOptionalStringProperty(
            builder,
            fc => fc.Description,
            EntityConfigurationConstants.LongStringLength);

        // Configure display order with default value
        builder.Property(fc => fc.DisplayOrder)
            .HasDefaultValue(0);

        // Configure required relationship to Forum
        ConfigureRequiredRelationship(
            builder,
            fc => fc.Forum,
            fc => fc.ForumId,
            "Forum");

        // Configure boolean property with index
        ConfigureBooleanPropertyWithIndex(
            builder,
            fc => fc.IsActive,
            "IsActive");

        // Configure composite unique index for forum and name
        builder.HasIndex(fc => new { fc.ForumId, fc.Name })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.UniqueIndexPattern,
                nameof(ForumCategory),
                "Forum_Name"))
            .IsUnique();

        // Configure index for forum ID
        builder.HasIndex(fc => fc.ForumId)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(ForumCategory),
                "ForumId"));
    }
}
