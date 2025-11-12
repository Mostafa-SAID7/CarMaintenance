using CommunityCar.Domain.Entities.Community.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommunityCar.Infrastructure.Data.Configurations.Community;

/// <summary>
/// Entity configuration for the Post entity
/// </summary>
public class PostConfiguration : BaseEntityConfiguration<Post>
{
    /// <summary>
    /// Configures the Post entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public override void Configure(EntityTypeBuilder<Post> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure base entity properties
        ConfigureBaseEntity(builder);

        // Configure required content properties
        ConfigureRequiredStringProperty(
            builder,
            p => p.Title,
            300, // Reasonable title length
            EntityConfigurationConstants.TitleProperty);

        ConfigureRequiredStringProperty(
            builder,
            p => p.Content,
            EntityConfigurationConstants.MaxContentLength,
            EntityConfigurationConstants.ContentProperty);

        // Configure optional content properties
        ConfigureOptionalStringProperty(
            builder,
            p => p.Excerpt,
            EntityConfigurationConstants.LongStringLength);

        ConfigureOptionalStringProperty(
            builder,
            p => p.Tags,
            100); // Reasonable tags string length

        ConfigureOptionalStringProperty(
            builder,
            p => p.ModerationReason,
            EntityConfigurationConstants.LongStringLength);

        // Configure relationships
        ConfigureRestrictRelationship(
            builder,
            p => p.Author,
            p => p.AuthorId,
            "Author");

        ConfigureRequiredRelationship(
            builder,
            p => p.Forum,
            p => p.ForumId,
            "Forum");

        ConfigureSetNullRelationship(
            builder,
            p => p.Category,
            p => p.CategoryId,
            "Category");

        // Configure boolean properties with indexes
        ConfigureBooleanPropertyWithIndex(
            builder,
            p => p.IsPinned,
            "IsPinned");

        ConfigureBooleanPropertyWithIndex(
            builder,
            p => p.IsLocked,
            "IsLocked");

        ConfigureBooleanPropertyWithIndex(
            builder,
            p => p.IsFeatured,
            "IsFeatured");

        ConfigureBooleanPropertyWithIndex(
            builder,
            p => p.IsApproved,
            "IsApproved");

        // Configure DateTime properties with indexes
        ConfigureUtcDateTimeProperty(
            builder,
            p => p.LastActivityAt,
            "LastActivityAt");

        // Configure additional indexes for common queries
        builder.HasIndex(p => new { p.ForumId, p.IsApproved, p.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Post),
                "Forum_Approved_CreatedAt"));

        builder.HasIndex(p => new { p.AuthorId, p.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Post),
                "Author_CreatedAt"));

        builder.HasIndex(p => new { p.IsFeatured, p.CreatedAt })
            .HasDatabaseName(string.Format(
                EntityConfigurationConstants.IndexPattern,
                nameof(Post),
                "Featured_CreatedAt"));
    }
}
