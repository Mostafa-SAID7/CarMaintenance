using CommunityCar.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace CommunityCar.Infrastructure.Data.Configurations;

/// <summary>
/// Base class for entity type configurations providing common functionality
/// </summary>
/// <typeparam name="TEntity">The entity type being configured</typeparam>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Configures the entity of type <typeparamref name="TEntity"/>
    /// </summary>
    /// <param name="builder">The builder being used to construct the entity type model</param>
    public abstract void Configure(EntityTypeBuilder<TEntity> builder);

    /// <summary>
    /// Configures common properties for entities inheriting from BaseEntity
    /// </summary>
    /// <param name="builder">The builder being used to construct the entity type model</param>
    protected virtual void ConfigureBaseEntity(EntityTypeBuilder<TEntity> builder)
    {
        if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
        {
            var baseBuilder = builder as EntityTypeBuilder<BaseEntity>;
            baseBuilder?.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAdd();

            baseBuilder?.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .ValueGeneratedOnAddOrUpdate();

            baseBuilder?.Property(e => e.IsDeleted)
                .HasDefaultValue(false);

            baseBuilder?.HasIndex(e => e.IsDeleted);
            baseBuilder?.HasIndex(e => e.CreatedAt);
            baseBuilder?.HasIndex(e => e.UpdatedAt);
        }
    }

    /// <summary>
    /// Configures a required string property with maximum length
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="propertyExpression">Expression selecting the property</param>
    /// <param name="maxLength">Maximum length of the string</param>
    /// <param name="propertyName">Name of the property for error messages</param>
    protected void ConfigureRequiredStringProperty(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, string?>> propertyExpression,
        int maxLength,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        builder.Property(propertyExpression)
            .HasMaxLength(maxLength)
            .IsRequired();

        builder.HasIndex(propertyExpression)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_{propertyName}")
            .IsUnique();
    }

    /// <summary>
    /// Configures an optional string property with maximum length
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="propertyExpression">Expression selecting the property</param>
    /// <param name="maxLength">Maximum length of the string</param>
    protected void ConfigureOptionalStringProperty(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, string?>> propertyExpression,
        int maxLength)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);

        builder.Property(propertyExpression)
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configures a foreign key relationship with cascade delete behavior
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="navigationExpression">Expression selecting the navigation property</param>
    /// <param name="foreignKeyExpression">Expression selecting the foreign key property</param>
    /// <param name="relationshipName">Name of the relationship for error messages</param>
    protected void ConfigureRequiredRelationship<TPrincipal>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TPrincipal?>> navigationExpression,
        Expression<Func<TEntity, object?>> foreignKeyExpression,
        string relationshipName)
        where TPrincipal : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(navigationExpression);
        ArgumentNullException.ThrowIfNull(foreignKeyExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationshipName);

        builder.HasOne(navigationExpression)
            .WithMany()
            .HasForeignKey(foreignKeyExpression)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasIndex(foreignKeyExpression)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_{relationshipName}Id");
    }

    /// <summary>
    /// Configures a foreign key relationship with restrict delete behavior
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="navigationExpression">Expression selecting the navigation property</param>
    /// <param name="foreignKeyExpression">Expression selecting the foreign key property</param>
    /// <param name="relationshipName">Name of the relationship for error messages</param>
    protected void ConfigureRestrictRelationship<TPrincipal>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TPrincipal?>> navigationExpression,
        Expression<Func<TEntity, object?>> foreignKeyExpression,
        string relationshipName)
        where TPrincipal : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(navigationExpression);
        ArgumentNullException.ThrowIfNull(foreignKeyExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationshipName);

        builder.HasOne(navigationExpression)
            .WithMany()
            .HasForeignKey(foreignKeyExpression)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(foreignKeyExpression)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_{relationshipName}Id");
    }

    /// <summary>
    /// Configures a foreign key relationship with set null delete behavior
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="navigationExpression">Expression selecting the navigation property</param>
    /// <param name="foreignKeyExpression">Expression selecting the foreign key property</param>
    /// <param name="relationshipName">Name of the relationship for error messages</param>
    protected void ConfigureSetNullRelationship<TPrincipal>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TPrincipal?>> navigationExpression,
        Expression<Func<TEntity, object?>> foreignKeyExpression,
        string relationshipName)
        where TPrincipal : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(navigationExpression);
        ArgumentNullException.ThrowIfNull(foreignKeyExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationshipName);

        builder.HasOne(navigationExpression)
            .WithMany()
            .HasForeignKey(foreignKeyExpression)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(foreignKeyExpression)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_{relationshipName}Id");
    }

    /// <summary>
    /// Configures a boolean property with an index
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="propertyExpression">Expression selecting the boolean property</param>
    /// <param name="propertyName">Name of the property for index naming</param>
    protected void ConfigureBooleanPropertyWithIndex(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, bool>> propertyExpression,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        builder.Property(propertyExpression)
            .HasDefaultValue(false);

        builder.HasIndex(propertyExpression)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_{propertyName}");
    }

    /// <summary>
    /// Configures a DateTime property with UTC default value
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    /// <param name="propertyExpression">Expression selecting the DateTime property</param>
    /// <param name="propertyName">Name of the property for index naming</param>
    protected void ConfigureUtcDateTimeProperty(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, DateTime>> propertyExpression,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(propertyExpression);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        builder.Property(propertyExpression)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(propertyExpression)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_{propertyName}");
    }
}

/// <summary>
/// Constants used across entity configurations
/// </summary>
public static class EntityConfigurationConstants
{
    // Common string length limits
    public const int ShortStringLength = 50;
    public const int MediumStringLength = 200;
    public const int LongStringLength = 500;
    public const int ExtraLongStringLength = 1000;
    public const int MaxContentLength = 10000;

    // Common property names
    public const string CreatedAtProperty = "CreatedAt";
    public const string UpdatedAtProperty = "UpdatedAt";
    public const string IsDeletedProperty = "IsDeleted";
    public const string TitleProperty = "Title";
    public const string ContentProperty = "Content";
    public const string DescriptionProperty = "Description";
    public const string NameProperty = "Name";

    // SQL defaults
    public const string UtcDateTimeDefault = "GETUTCDATE()";
    public const string FalseDefault = "0";

    // Index naming patterns
    public const string UniqueIndexPattern = "UX_{0}_{1}";
    public const string IndexPattern = "IX_{0}_{1}";
    public const string ForeignKeyIndexPattern = "IX_{0}_{1}Id";
}