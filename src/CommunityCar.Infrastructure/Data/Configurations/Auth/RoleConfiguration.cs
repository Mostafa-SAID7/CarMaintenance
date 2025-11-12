using CommunityCar.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Auth;

/// <summary>
/// Entity configuration for the Role entity
/// </summary>
public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    /// <summary>
    /// Configures the Role entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure optional description property
        ConfigureOptionalStringProperty(
            builder,
            r => r.Description,
            EntityConfigurationConstants.MediumStringLength);

        // Configure unique index for role name (inherited from Identity)
        builder.HasIndex(r => r.Name)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.UniqueIndexPattern,
                nameof(Role),
                EntityConfigurationConstants.NameProperty))
            .IsUnique();

        // Configure index for normalized name (inherited from Identity)
        builder.HasIndex(r => r.NormalizedName)
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Role),
                "NormalizedName"));
    }
}
